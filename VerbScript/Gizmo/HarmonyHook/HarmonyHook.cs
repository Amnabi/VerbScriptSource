using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VerbScript {

    [DefOf]
    public static class AVVerbDefOf{
        public static JobDef UseVerbOnThingContinuous;
    }

    [StaticConstructorOnStartup]
    public static class Harmony_VerbScriptHook {
        public static HashSet<ThingDef> SA_AnimThingDefs = new HashSet<ThingDef>();
        static Harmony_VerbScriptHook(){
            foreach(ThingDef td in DefDatabase<ThingDef>.AllDefs){
                if(td.HasComp(typeof(Comp_VerbHolder))) {
                    SA_AnimThingDefs.Add(td);
                }
            }

            Harmony harmony = new Harmony("Amnabi.AnimationHook");
            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Pawn_JobTracker), "StartJob"),
                null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "JobStartHook")
                );
            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Pawn_JobTracker), "CleanupCurrentJob"),
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "JobCleanupHook"),
                null
                );
            harmony.Patch(
                AccessTools.DeclaredPropertyGetter(typeof(Pawn_DraftController), "Drafted"),
                null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "DraftHook")
                );
            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Pawn_PathFollower), "StartPath"),
                null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "StartPathHook")
                );
            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Pawn_PathFollower), "StopDead"),
                null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "StopDeadPathHook")
                );
            
            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Pawn_StanceTracker), "SetStance"),
                null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "SetStance")
                );
            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Pawn_HealthTracker), "RemoveHediff"),
                null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "HediffChangeListner")
                );
            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Pawn_HealthTracker), "AddHediff", new Type[]{ 
                    typeof(Hediff), 
                    typeof(BodyPartRecord), 
                    typeof(DamageInfo?), 
                    typeof(DamageWorker.DamageResult)
                }),
                null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "HediffChangeListnerAdd")
                );
            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(TraitSet), "GainTrait"),
                null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "TraitChangeListnerAdd")
                );
            
			harmony.Patch(
                AccessTools.Method(typeof(CameraShaker), "DoShake", null, null), 
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "DoShakePrefix", null), null, null, null);

			harmony.Patch(
                AccessTools.Method(typeof(Pawn), "GetGizmos"),
				null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), nameof(GizmoPass)),
                null);

			harmony.Patch(
                AccessTools.Method(typeof(Targeter), "GetTargetingVerb", null, null),
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "GetTargetingVerb", null), null, null);
			harmony.Patch(
                AccessTools.Method(typeof(Verb), "OrderForceTarget", null, null),
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "OrderForceTargetVerb", null), null, null);
			harmony.Patch(
                AccessTools.Method(typeof(Targeter), "ProcessInputEvents", null, null),
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "ProcessInputEventsPOST", null), null, null);
			harmony.Patch(
                AccessTools.Method(typeof(MentalStateHandler), "ClearMentalStateDirect", null, null),
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "ClearMentalStateDirect_Hook", null), null, null);
            harmony.Patch(
                AccessTools.Method(typeof(MentalStateHandler), "TryStartMentalState", null, null),
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "TryStartMentalState_Hook", null), null, null);

            
            ///
            
            harmony.Patch(
                AccessTools.Method(typeof(Pawn_EquipmentTracker), "AddEquipment", null, null), null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "AddEquipment_Hook", null), null, null);
            harmony.Patch(
                AccessTools.Method(typeof(Pawn_EquipmentTracker), "Remove", null, null), null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "Remove_Hook", null), null, null);
            harmony.Patch(
                AccessTools.Method(typeof(Pawn_EquipmentTracker), "TryDropEquipment", null, null), null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "TryDropEquipment_Hook", null), null, null);
            harmony.Patch(
                AccessTools.Method(typeof(Pawn_EquipmentTracker), "TryTransferEquipmentToContainer", null, null), null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "TryTransferEquipmentToContainer_Hook", null), null, null);
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(Verb), "EquipmentSource"),
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "EquipmentSource_PRE", null), null, null);
            /**harmony.Patch(
                AccessTools.DeclaredPropertySetter(typeof(Pawn_EquipmentTracker), "Primary"), null,
                new HarmonyMethod(typeof(Harmony_VerbScriptHook), "Primary_Hook", null), null, null);
            **/

            WarmupPatch(typeof(Verb), harmony);
            foreach(Type type in typeof(Verb).AllSubclasses()){
                WarmupPatch(type, harmony);
            }

            foreach(ThingDef td in DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null && x.race.Humanlike)){
                if(td.comps == null){
                    td.comps = new List<CompProperties>();
                }
                td.comps.Add(new CompProperties_VerbHolder());
            }
        }
		public static void RemoveTrait(Pawn pawn, TraitDef def){
            TraitSet ts = pawn.story.traits;
			if(!ts.HasTrait(def)){
				return;
			}
            Trait tt = ts.allTraits.First(x => x.def == def);
            int degree = tt.Degree;
			ts.allTraits.Remove(tt);
			pawn.Notify_DisabledWorkTypesChanged();
			if (pawn.skills != null)
			{
				pawn.skills.Notify_SkillDisablesChanged();
			}
			if (!pawn.Dead && pawn.RaceProps.Humanlike && pawn.needs.mood != null)
			{
				pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
			}
			MeditationFocusTypeAvailabilityCache.ClearFor(pawn);
            Comp_VerbHolder caa = pawn.TryGetComp<Comp_VerbHolder>();
            if(caa != null && !caa.needsRefreshVerbs){
                caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Trait.needsRefresh(caa.root.quickData, (def.index << 16) | (degree + 256));
            }
		}

		public static void AddEquipment_Hook(Pawn_EquipmentTracker __instance, ThingWithComps newEq){
            if(newEq != null){
                Comp_VerbHolder caa = __instance.pawn.TryGetComp<Comp_VerbHolder>();
                if(caa != null && !caa.needsRefreshVerbs){
                    caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Equipment.needsRefresh(caa.root.quickData, newEq.def.index);
                }
            }
        }
		public static void Remove_Hook(Pawn_EquipmentTracker __instance, ThingWithComps eq){
            if(eq != null){
                Comp_VerbHolder caa = __instance.pawn.TryGetComp<Comp_VerbHolder>();
                if(caa != null && !caa.needsRefreshVerbs){
                    caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Equipment.needsRefresh(caa.root.quickData, eq.def.index);
                }
            }
        }
		public static void TryDropEquipment_Hook(Pawn_EquipmentTracker __instance, ThingWithComps eq){
            if(eq != null){
                Comp_VerbHolder caa = __instance.pawn.TryGetComp<Comp_VerbHolder>();
                if(caa != null && !caa.needsRefreshVerbs){
                    caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Equipment.needsRefresh(caa.root.quickData, eq.def.index);
                }
            }
        }
		public static void TryTransferEquipmentToContainer_Hook(Pawn_EquipmentTracker __instance, ThingWithComps eq){
            if(eq != null){
                Comp_VerbHolder caa = __instance.pawn.TryGetComp<Comp_VerbHolder>();
                if(caa != null && !caa.needsRefreshVerbs){
                    caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Equipment.needsRefresh(caa.root.quickData, eq.def.index);
                }
            }
        }

        public static bool EquipmentSource_PRE(Verb __instance, ref ThingWithComps __result){
            if(SA_EquipmentSourceSwitch){
                __result = __instance.CasterPawn;
                return false;
            }
            return true;
        }

        public static void WarmupPatch(Type type, Harmony harmony){
            MethodInfo methInfo = AccessTools.Method(type, "WarmupComplete");
            if(methInfo != null && methInfo.DeclaringType == type && !methInfo.IsAbstract){
                SA_TypeTranspilerNow = type;
                //Log.Message("Autopatching " + type);
                harmony.Patch(
                    methInfo,
                    null,
                    null,
                    new HarmonyMethod(typeof(Harmony_VerbScriptHook), nameof(WarmupComplete_Transpiler)),
                    new HarmonyMethod(typeof(Harmony_VerbScriptHook), nameof(WarmupComplete_Final))
                );
            }
        }

        public static void CooldownCheck(Verb verb){
            SA_EquipmentSourceSwitch = true;
            //Log.Warning("Checking cooldown " + verb);
            try{
                if(verb.caster is Pawn casterPawn){
                    Comp_VerbHolder cvh = casterPawn.TryGetComp<Comp_VerbHolder>();
                    if(cvh != null && cvh.verbToVerbData.TryGetValue(verb, out VerbData vdOut)){
                        //Log.Warning("Cooldown set " + vdOut);
                        cvh.lastUsedTicks.setSafe(vdOut, Find.TickManager.TicksGame);
                        if(vdOut.fire != null){
				            ExecuteStackContext newContext = ExecuteStackContext.nextScriptExecutorContext();
                            newContext.verbScript = vdOut.fire;
				            newContext.Param_TargetInfo = verb.CurrentTarget;
				            newContext.Param_ExtraTargetInfo = cvh.IDtoExtraTargets.getSafe(vdOut.ID, null);
				            cvh.activeExecuteStacks.Add(newContext);
                        }
                    }
                }
            }catch (Exception e){
				Log.Warning(e.Message + "\n" + e.StackTrace, false);
			}
        }

        public static Type SA_TypeTranspilerNow;
        public static Dictionary<Type, MethodInfo> SA_WarmupComplete_Call = new Dictionary<Type, MethodInfo>();
	    public static MethodInfo SA_CheckCooldown = AccessTools.DeclaredMethod(typeof(Harmony_VerbScriptHook), "CooldownCheck");
        public static bool SA_EquipmentSourceSwitch;
        public static void WarmupComplete_Final(){
            SA_EquipmentSourceSwitch = false;
        }
        public static IEnumerable<CodeInstruction> WarmupComplete_Transpiler(IEnumerable<CodeInstruction> instructions){
            MethodInfo method = null;
            Type basee = SA_TypeTranspilerNow.BaseType;
            List<CodeInstruction> CIPrev = instructions.ToList();
            bool hasBaseMethod = false;
            if(basee != null){
                method = SA_WarmupComplete_Call.getInitSafe(basee, delegate(){ return basee.GetMethod("WarmupComplete"); });
                if(method != null){
                    for(int i = 0; i < CIPrev.Count && !hasBaseMethod; i++){
                        CodeInstruction instCheck = CIPrev[i];
                        if(instCheck.operand == method){
                            hasBaseMethod = true;
                            //Log.Warning("Detected base method in " + SA_TypeTranspilerNow + " / " + method?.DeclaringType);
                        }
                    }
                }
            }else{
            }
            if(!hasBaseMethod){
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, Harmony_VerbScriptHook.SA_CheckCooldown);
            }

            for(int i = 0; i < CIPrev.Count; i++){
				CodeInstruction ci = CIPrev[i];
                yield return ci;
			}
            yield break;
        }
        
        public static bool SA_DisableShake;
        public static bool DoShakePrefix(CameraShaker __instance){
            return !SA_DisableShake;
        }
        
		public static bool GetTargetingVerb(Targeter __instance, Pawn pawn, ref Verb __result){
            if(Command_VerbScript.SA_KeyReference == __instance.targetingSource && Command_VerbScript.SA_OneToOne.TryGetValue(pawn, out Command_VerbScript cvs)){
                __result = cvs.verb;
                return false;
            }
            return true;
        }

        public static void RecursiveTargeting(Verb verb, Pawn caster, VerbData vd, Comp_VerbHolder cvb, int i){
			TargetingParameters targetingParameters = TargetingParameters.ForAttackAny();
            targetingParameters.canTargetLocations = true;
            targetingParameters.validator = delegate(TargetInfo x){
                LocalTargetInfoListed ha = cvb.IDtoExtraTargets.getInitSafe(vd.ID, FreePool<LocalTargetInfoListed>.next);
                ha.elements.Clear();
                foreach(LocalTargetInfo ll in SA_lti){
                    ha.elements.Add(new LocalTargetInfoElement(){ localTargetInfo = ll });
                }
				ExecuteStackContext newContext = ExecuteStackContext.nextScriptExecutorContext();
                newContext.verbScript = vd.extraTarget[i].allowTarget;
				newContext.Param_TargetInfo = (LocalTargetInfo)x;
				newContext.Param_ExtraTargetInfo = ha;
				return newContext.tryExecuteEnum0Delay(caster).singular().recast<bool>();
            };
			Find.Targeter.BeginTargeting(targetingParameters, delegate(LocalTargetInfo t){
                SA_lti.Add(t);
			}, delegate(LocalTargetInfo doj){
				if(vd.extraTarget[i].cellHighlight != null){
                    LocalTargetInfoListed ha = cvb.IDtoExtraTargets.getInitSafe(vd.ID, FreePool<LocalTargetInfoListed>.next);
                    ha.elements.Clear();
                    foreach(LocalTargetInfo ll in SA_lti){
                        ha.elements.Add(new LocalTargetInfoElement(){ localTargetInfo = ll });
                    }
					ExecuteStackContext.SA_StaticContext.clear();
					ExecuteStackContext.SA_StaticContext.verbScript = vd.extraTarget[i].cellHighlight;
					ExecuteStackContext.SA_StaticContext.Param_TargetInfo = doj;
					ExecuteStackContext.SA_StaticContext.Param_ExtraTargetInfo = ha;
					ExecuteStackContext.SA_StaticContext.thingVariableHolder = cvb.variableHolder;
					cellIterator.Clear();
					foreach(object obj in ExecuteStackContext.SA_StaticContext.tryExecuteEnum0Delay(caster)){
						cellIterator.Add(obj.recast<IntVec3>());
					}
					GenDraw.DrawFieldEdges(cellIterator);
					cellIterator.Clear();
				    GenDraw.DrawTargetHighlight(doj);
				}
            }, delegate(LocalTargetInfo lti){ return true; },
            caster, delegate(){
                if(vd.extraTarget.Count - 1 == i){
			        if(vd.verbProps.IsMeleeAttack){
				        Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, SA_lti[0]);
				        job.playerForced = true;
				        Pawn pawn = SA_lti[0].Thing as Pawn;
				        if (pawn != null)
				        {
					        job.killIncappedTarget = pawn.Downed;
				        }
				        caster.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				        return;
			        }
			        float num = vd.verbProps.EffectiveMinRange(SA_lti[0], caster);
			        if((float)caster.Position.DistanceToSquared(SA_lti[0].Cell) < num * num && caster.Position.AdjacentTo8WayOrInside(SA_lti[0].Cell))
			        {
				        Messages.Message("MessageCantShootInMelee".Translate(), verb.CasterPawn, MessageTypeDefOf.RejectInput, false);
				        return;
			        }
			        Job job2 = JobMaker.MakeJob(cvb.verbToVerbData[verb].repeatVerb? AVVerbDefOf.UseVerbOnThingContinuous : JobDefOf.UseVerbOnThingStatic);
			        job2.verbToUse = verb;
			        job2.targetA = SA_lti[0];
                    job2.maxNumStaticAttacks = 1000000;
			        job2.endIfCantShootInMelee = true;
			        verb.CasterPawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);

                    LocalTargetInfoListed ha = cvb.IDtoExtraTargets.getInitSafe(vd.ID, FreePool<LocalTargetInfoListed>.next);
                    ha.elements.Clear();
                    foreach(LocalTargetInfo ll in SA_lti){
                        ha.elements.Add(new LocalTargetInfoElement(){ localTargetInfo = ll });
                    }

                    if(vd.init != null){
				        ExecuteStackContext newContext = ExecuteStackContext.nextScriptExecutorContext();
                        newContext.verbScript = vd.init;
				        newContext.Param_TargetInfo = verb.CurrentTarget;
				        newContext.Param_ExtraTargetInfo = ha;
				        cvb.activeExecuteStacks.Add(newContext);
                    }
                }else{
                    RecursiveTargeting(verb, caster, vd, cvb, i + 1);
                }
			}, vd.extraTarget[i].MouseTexture);
        }
        
		public static List<IntVec3> cellIterator = new List<IntVec3>();
        public static List<LocalTargetInfo> SA_lti = new List<LocalTargetInfo>();
        public static int SA_OFTVIndex = 0;
        public static Action SA_PostMultiTargetAction;
        public static void ProcessInputEventsPOST(){
            if(SA_PostMultiTargetAction != null){
                SA_PostMultiTargetAction();
                SA_PostMultiTargetAction = null;
            }

        }

		public static bool OrderForceTargetVerb(LocalTargetInfo target, Verb __instance){
            //Log.Warning("__instance" + __instance);
            Thing caster = __instance.caster;
            if(caster is Pawn casterPawn){
                Comp_VerbHolder cvs = casterPawn.TryGetComp<Comp_VerbHolder>();
                if(cvs != null && cvs.verbToVerbData.TryGetValue(__instance, out VerbData vdOut)){
                    if(vdOut.extraTarget != null){
                        SA_PostMultiTargetAction = delegate(){
                            SA_OFTVIndex = 0;
                            SA_lti.Clear();
                            SA_lti.Add(target);
                            //vdOut.extraTarget.
                            RecursiveTargeting(__instance, casterPawn, vdOut, cvs, SA_OFTVIndex);
                        };
				        return false;
                    }else{
			            if(__instance.verbProps.IsMeleeAttack){
				            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
				            job.playerForced = true;
				            Pawn pawn = target.Thing as Pawn;
				            if (pawn != null)
				            {
					            job.killIncappedTarget = pawn.Downed;
				            }
				            __instance.CasterPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				            return false;
			            }
			            float num = __instance.verbProps.EffectiveMinRange(target, __instance.CasterPawn);
			            if ((float)__instance.CasterPawn.Position.DistanceToSquared(target.Cell) < num * num && __instance.CasterPawn.Position.AdjacentTo8WayOrInside(target.Cell))
			            {
				            Messages.Message("MessageCantShootInMelee".Translate(), __instance.CasterPawn, MessageTypeDefOf.RejectInput, false);
				            return false;
			            }
			            Job job2 = JobMaker.MakeJob(cvs.verbToVerbData[__instance].repeatVerb? AVVerbDefOf.UseVerbOnThingContinuous : JobDefOf.UseVerbOnThingStatic);
			            job2.verbToUse = __instance;
			            job2.targetA = target;
                        job2.maxNumStaticAttacks = 1000000;
			            job2.endIfCantShootInMelee = true;
			            __instance.CasterPawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);

                        if(vdOut.init != null){
				            ExecuteStackContext newContext = ExecuteStackContext.nextScriptExecutorContext();
                            newContext.verbScript = vdOut.init;
				            newContext.Param_TargetInfo = __instance.CurrentTarget;
				            cvs.activeExecuteStacks.Add(newContext);
                        }
				        return false;
                    }
                }
            }
            return true;
        }
        public static void SetStance(Pawn_StanceTracker __instance, Stance newStance){
            try{
                if(__instance.curStance is Stance_Warmup wu){
                    if(wu.verb.state != VerbState.Bursting){
                        Comp_VerbHolder compVerbHolder = __instance.pawn.TryGetComp<Comp_VerbHolder>();
                        if(compVerbHolder != null && compVerbHolder.verbToVerbData.TryGetValue(wu.verb, out VerbData vdOut)){
                            if(vdOut.cancel != null){
				                ExecuteStackContext newContext = ExecuteStackContext.nextScriptExecutorContext();
                                newContext.verbScript = vdOut.cancel;
				                newContext.Param_TargetInfo = wu.verb.CurrentTarget;
				                newContext.Param_ExtraTargetInfo = compVerbHolder.IDtoExtraTargets.getSafe(vdOut.ID, null);
				                compVerbHolder.activeExecuteStacks.Add(newContext);
                            }
                        }

                    }
                }
            }catch (Exception e){
				Log.Warning(e.Message + "\n" + e.StackTrace, false);
			}
            /**Log.Warning(__instance.pawn + " / " + newStance);
            if(newStance is Stance_Warmup aeong){
                bool bb = aeong.focusTarg.HasThing && (!aeong.focusTarg.Thing.Spawned || aeong.verb == null || !aeong.verb.CanHitTargetFrom(__instance.pawn.Position, aeong.focusTarg));
                Log.Warning(" i " + bb + " / " + aeong.verb.targetParams.canTargetSelf);
            }**/
        }
        
		public static IEnumerable<Gizmo> GizmoPass(IEnumerable<Gizmo> values, Pawn __instance){
			foreach(Gizmo giz in values){
				yield return giz;
			}
			//if(__instance.IsColonistPlayerControlled){
			if(__instance.RaceProps.Humanlike){
                Comp_VerbHolder compVerbHolder = __instance.TryGetComp<Comp_VerbHolder>();
                if(compVerbHolder != null){
				    foreach(Gizmo giz in compVerbHolder.getVerbCommands()){
                        yield return giz;
                    }
                }
			}
			yield break;
		}

        //FLAG

        public static bool ClearMentalStateDirect_Hook(MentalStateHandler __instance, MentalState ___curStateInt, Pawn ___pawn){
            try{
                if(___curStateInt != null){
                    Comp_VerbHolder caa =___pawn.TryGetComp<Comp_VerbHolder>();
                    if(caa != null && !caa.needsRefreshVerbs){
                        caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_MentalState.needsRefresh(caa.root.quickData, ___curStateInt.def.index);
                    }
                }
            }catch (Exception e){
				Log.Warning(e.Message + "\n" + e.StackTrace, false);
			}
            return true;
        }
        public static bool TryStartMentalState_Hook(MentalStateHandler __instance, MentalStateDef stateDef, MentalState ___curStateInt, Pawn ___pawn){
            try{
                if(stateDef != null){
                    Comp_VerbHolder caa =___pawn.TryGetComp<Comp_VerbHolder>();
                    if(caa != null && !caa.needsRefreshVerbs){
                        caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_MentalState.needsRefresh(caa.root.quickData, stateDef.index) | (___curStateInt != null && VerbQuickFilter.SAQFI_MentalState.needsRefresh(caa.root.quickData, ___curStateInt.def.index));
                    }
                }
            }catch (Exception e){
				Log.Warning(e.Message + "\n" + e.StackTrace, false);
			}
            return true;
        }
        public static void TraitChangeListnerAdd(TraitSet __instance, Trait trait, Pawn ___pawn){
            try{
                Comp_VerbHolder caa =___pawn.TryGetComp<Comp_VerbHolder>();
                if(caa != null && !caa.needsRefreshVerbs){
                    caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Trait.needsRefresh(caa.root.quickData, (trait.def.index << 16) | (trait.Degree + 256));
                }
            }catch (Exception e){
				Log.Warning(e.Message + "\n" + e.StackTrace, false);
			}
        }
        public static void HediffChangeListner(Pawn_HealthTracker __instance, Hediff hediff, Pawn ___pawn){
            try{
                Comp_VerbHolder caa =___pawn.TryGetComp<Comp_VerbHolder>();
                if(caa != null && !caa.needsRefreshVerbs){
                    if(hediff.Part != null){
                        caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Hediff.needsRefresh(caa.root.quickData, (hediff.Part.def.index << 16) | hediff.def.index);
                    }
                    if(!caa.needsRefreshVerbs){
                        caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Hediff.needsRefresh(caa.root.quickData, 0x7FFF0000 | hediff.def.index);
                    }
                }
            }catch (Exception e){
				Log.Warning(e.Message + "\n" + e.StackTrace, false);
			}
        }
        public static void HediffChangeListnerAdd(Pawn_HealthTracker __instance, Hediff hediff, Pawn ___pawn){
            try{
                Comp_VerbHolder caa =___pawn.TryGetComp<Comp_VerbHolder>();
                if(caa != null && !caa.needsRefreshVerbs){
                    if(hediff.Part != null){
                        caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Hediff.needsRefresh(caa.root.quickData, (hediff.Part.def.index << 16) | hediff.def.index);
                    }
                    if(!caa.needsRefreshVerbs){
                        caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Hediff.needsRefresh(caa.root.quickData, 0x7FFF0000 | hediff.def.index);
                    }
                }
            }catch (Exception e){
				Log.Warning(e.Message + "\n" + e.StackTrace, false);
			}
        }
        public static void StopDeadPathHook(Pawn_PathFollower __instance, Pawn ___pawn){
            try{
                Comp_VerbHolder caa =___pawn.TryGetComp<Comp_VerbHolder>();
                if(caa != null && !caa.needsRefreshVerbs){
                    caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Moving.needsRefresh(caa.root.quickData, 0) | VerbQuickFilter.SAQFI_Moving.needsRefresh(caa.root.quickData, 1);
                }
            }catch (Exception e){
				Log.Warning(e.Message + "\n" + e.StackTrace, false);
			}
        }
        public static void StartPathHook(Pawn_PathFollower __instance, Pawn ___pawn){
            try{
                Comp_VerbHolder caa =___pawn.TryGetComp<Comp_VerbHolder>();
                if(caa != null && !caa.needsRefreshVerbs){
                    caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Moving.needsRefresh(caa.root.quickData, 0) | VerbQuickFilter.SAQFI_Moving.needsRefresh(caa.root.quickData, 1);
                }
            }catch (Exception e){
				Log.Warning(e.Message + "\n" + e.StackTrace, false);
			}
        }
        public static void DraftHook(Pawn_DraftController __instance, Pawn ___pawn){
            try{
                Comp_VerbHolder caa =___pawn.TryGetComp<Comp_VerbHolder>();
                if(caa != null && !caa.needsRefreshVerbs){
                    caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Moving.needsRefresh(caa.root.quickData, 0) | VerbQuickFilter.SAQFI_Moving.needsRefresh(caa.root.quickData, 1);
                }
            }catch (Exception e){
				Log.Warning(e.Message + "\n" + e.StackTrace, false);
			}
        }
        public static bool JobCleanupHook(Pawn_JobTracker __instance, Pawn ___pawn){
            try{
                Comp_VerbHolder caa =___pawn.TryGetComp<Comp_VerbHolder>();
                if(caa != null){
                    if(!caa.needsRefreshVerbs){
                        if(caa.lastJobDef != null){
                            caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Job.needsRefresh(caa.root.quickData, caa.lastJobDef.index);
                        }else{
                        }
                    }
                    Job curJob = ___pawn.CurJob;
                    caa.lastJobDef = curJob == null? null : ___pawn.CurJob.def;
                }
            }catch (Exception e){
				Log.Warning(e.Message + "\n" + e.StackTrace, false);
			}
            return true;
        }

        public static void JobStartHook(Pawn_JobTracker __instance, Pawn ___pawn){
            try{
                Comp_VerbHolder caa =___pawn.TryGetComp<Comp_VerbHolder>();
                if(caa != null){
                    Job curJob = ___pawn.CurJob;
                    if (!caa.needsRefreshVerbs && !((curJob == null && caa.lastJobDef == null) || (curJob.def == caa.lastJobDef))){
                        if(caa.lastJobDef != null){
                            if(curJob != null){
                                caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Job.needsRefresh(caa.root.quickData, curJob.def.index) | VerbQuickFilter.SAQFI_Job.needsRefresh(caa.root.quickData, caa.lastJobDef.index);
                            }else{
                                caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Job.needsRefresh(caa.root.quickData, caa.lastJobDef.index);
                            }
                        }else{
                            if(curJob != null){
                                caa.needsRefreshVerbs |= VerbQuickFilter.SAQFI_Job.needsRefresh(caa.root.quickData, curJob.def.index);
                            }else{
                            }
                        }
                    }
                    caa.lastJobDef = curJob == null? null : ___pawn.CurJob.def;
                }
            }catch (Exception e){
				Log.Warning(e.Message + "\n" + e.StackTrace, false);
			}
        }

    }
}
