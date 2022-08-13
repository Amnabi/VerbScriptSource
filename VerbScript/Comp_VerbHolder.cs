using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using Verse.AI;

namespace VerbScript {
	
    public class CompProperties_ScriptExecutor : CompProperties{
        public CompProperties_ScriptExecutor(){
            this.compClass = typeof(Comp_ScriptExecutor);
        }
		public VerbScript verbScript;
    }
    public class Comp_ScriptExecutor : ThingComp {
		public CompProperties_ScriptExecutor Props{
			get{
				return (CompProperties_ScriptExecutor)props;
			}
		}
		public int ticksFromTickerType{
			get{
				switch(this.parent.def.tickerType){
					case TickerType.Normal:{
						return 1;
					}
					case TickerType.Rare:{
						return 250;
					}
					case TickerType.Long:{
						return 2000;
					}
				}
				return 1;
			}
		}
        public override void CompTick() {
            base.CompTick();
			activeExecuteStack.thingVariableHolder = variableHolder;
			if(activeExecuteStack.tryExecute(parent, 1)){
				activeExecuteStack.free();
				parent.Destroy();
			}
        }
        public override void CompTickRare() {
            base.CompTickRare();
			activeExecuteStack.thingVariableHolder = variableHolder;
			if(activeExecuteStack.tryExecute(parent, 250)){
				activeExecuteStack.free();
				parent.Destroy();
			}
        }
        public override void CompTickLong() {
            base.CompTickLong();
			activeExecuteStack.thingVariableHolder = variableHolder;
			if(activeExecuteStack.tryExecute(parent, 2000)){
				activeExecuteStack.free();
				parent.Destroy();
			}
        }

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
			if(!respawningAfterLoad){
				ExecuteStackContext newContext = ExecuteStackContext.nextScriptExecutorContext();
				newContext.verbScript = Props.verbScript;
				activeExecuteStack = newContext;
			}
        }
        public override void PostExposeData(){
			base.PostExposeData();
			Scribe_Deep.Look<VariableHolder>(ref variableHolder, "variableHolder_ScrExecutor");
			Scribe_Deep.Look<ExecuteStackContext>(ref activeExecuteStack, "activeExecuteStack_ScrExecutor");
		}
		public ExecuteStackContext activeExecuteStack;
		public VariableHolder variableHolder = new VariableHolder();
	}
    public class CompProperties_VerbHolder : CompProperties{
        public CompProperties_VerbHolder(){
            this.compClass = typeof(Comp_VerbHolder);
        }
    }
    public class Comp_VerbHolder : ThingComp, IVerbOwner {
		public static FieldInfo FIAccess_verbs = AccessTools.DeclaredField(typeof(VerbTracker), "verbs");

		private Verb CreateVerb(VerbData vb){
			VerbProperties vP = vb.verbProps;
			Verb verb = (Verb)Activator.CreateInstance(vP.verbClass);
			verb.loadID = Verb.CalculateUniqueLoadID(this, vb.ID);
			verb.verbProps = vP;
			verb.verbTracker = verbTracker;
			verb.caster = this.parent;
			return verb;
		}

        public VerbTracker verbTracker;
        public Comp_VerbHolder(){
			this.verbTracker = new VerbTracker(this);
			FIAccess_verbs.SetValue(verbTracker, verbSync);
        }

        //VO
		string IVerbOwner.UniqueVerbOwnerID(){
			return "ScriptedVerbs_" + this.parent.ThingID;
		}
		bool IVerbOwner.VerbsStillUsableBy(Pawn p){
			return true;
		}
		Thing IVerbOwner.ConstantCaster{
			get{
				return this.parent;
			}
		}
		public VerbTracker VerbTracker{
			get{
				return this.verbTracker;
			}
		}

		public List<VerbProperties> VerbProperties{
			get{
				return null;
			}
		}

		public List<Tool> Tools{
			get{
				return null;
			}
		}

		ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef{
			get{
				return ImplementOwnerTypeDefOf.NativeVerb;
			}
		}

        private VerbSequenceRoot C_root;
        public VerbSequenceRoot root{
            get{
                if(C_root == null){
                    int i = this.parent.def.index;
                    VerbSequenceRoot cOut;
                    if(VerbData.SA_ThingDefIndexToTree.TryGetValue(i, out cOut)){
                        C_root = cOut;
                    }else{
						C_root = VerbData.SA_ThingDefIndexToTree[-1];
					}
                }
                return C_root;
            }
        }

		public IEnumerable<Gizmo> getVerbCommands(){
			Thing ownerThing = parent;
			int num;
			for (int i = 0; i < verbSync.Count; i = num + 1)
			{
				Verb verb = verbSync[i];
				if (verb.verbProps.hasStandardCommand)
				{
					yield return this.CreateVerbTargetCommand(ownerThing, verb);
				}
				num = i;
			}
		}

		private Command_VerbScript CreateVerbTargetCommand(Thing ownerThing, Verb verb){
			VerbData VD = this.verbToVerbData[verb];
			string failReason = FailReason(verb, VD, ownerThing as Pawn, this, false);
			Command_VerbScript command_VerbTarget = VD.noTarget? new Command_VerbScriptNonTarget() : new Command_VerbScriptTarget();
			command_VerbTarget.defaultDesc = VD.description;
			command_VerbTarget.defaultLabel = VD.label;
			command_VerbTarget.icon = ContentFinder<Texture2D>.Get(VD.icon, true);
			command_VerbTarget.verb = verb;
			command_VerbTarget.verbData = VD;
			command_VerbTarget.verbHolder = this;
			if (verb.caster.Faction != Faction.OfPlayer){
				command_VerbTarget.Disable("CannotOrderNonControlled".Translate());
			}
			else if (verb.CasterIsPawn){
				if (!verb.CasterPawn.drafter.Drafted){
					command_VerbTarget.Disable("IsNotDrafted".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
				}
			}
			if(!command_VerbTarget.disabled){
				command_VerbTarget.disabled = failReason != null;
				command_VerbTarget.disabledReason = failReason == null? "" : failReason.Translate();
			}
			return command_VerbTarget;
		}

		public Pawn pawn{
			get{
				return parent as Pawn;
			}
		}
		public Verb getCurrentVerb{
			get{
				Pawn p = pawn;
				if(p.stances != null && p.stances.curStance is Stance_Busy bz){
					return bz.verb;
				}
				Job job = p.CurJob;
				if(job != null && job.verbToUse != null){
					return job.verbToUse;
				}
				return null;
			}
		}
		
		public bool VerbUseValidNow(){
			Pawn p = (parent as Pawn);
			return p.Spawned && p.MapHeld != null && !p.Downed;
		}
		public bool isAutoCaster(){
			return !(parent as Pawn).IsColonistPlayerControlled;
		}
		public static IAttackTarget BestShootTargetFromCurrentPosition(Verb currentEffectiveVerb, IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null, float minDistance = 0f, float maxDistance = 9999f){
			if (currentEffectiveVerb == null)
			{
				Log.Error("BestShootTargetFromCurrentPosition with " + searcher.ToStringSafe<IAttackTargetSearcher>() + " who has no attack verb.", false);
				return null;
			}
			return AttackTargetFinder.BestAttackTarget(searcher, flags, validator, Mathf.Max(minDistance, currentEffectiveVerb.verbProps.minRange), Mathf.Min(maxDistance, currentEffectiveVerb.verbProps.range), default(IntVec3), float.MaxValue, false, false);
		}
        public override void CompTick() {
            base.CompTick();
            if(needsRefreshVerbs){
                tryRefreshVerbs();
            }

			if(parent.Spawned){
				for(int i = 0; i < activeExecuteStacks.Count; i++){
					activeExecuteStacks[i].thingVariableHolder = variableHolder;
					if(activeExecuteStacks[i].tryExecute(parent, 1)){
						activeExecuteStacks[i].free();
						activeExecuteStacks.RemoveAt(i);
						i -= 1;
					}
				}
			}
			/**if(Find.TickManager.TicksGame % 10 == 0 && isAutoCaster() && this.VerbUseValidNow()){
				Log.Warning(parent + " / " + aiVerbChecks.Count);
			}**/
			if(Find.TickManager.TicksGame % 10 == 0 && isAutoCaster() && this.VerbUseValidNow()){
				Verb verbNow = getCurrentVerb;
				if(verbNow == null || !this.verbToVerbData.ContainsKey(verbNow)){
					float highest = 0;
					Pawn bestTarget;
					foreach(VerbData vd in aiVerbChecks){
						Verb vb = verbDataToVerb[vd];
						//Log.Warning(parent + " / " + vd.def.defName);
						if(Comp_VerbHolder.FailReason(vb, vd, parent as Pawn, this, true) == null){
							if(vd.useDefaultAI){
								TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
								if (vb.IsIncendiary()){
									targetScanFlags |= TargetScanFlags.NeedNonBurning;
								}
								//Log.Warning("TryCheck");
								Thing thing = (Thing)BestShootTargetFromCurrentPosition(vb, pawn, targetScanFlags, x => x != null && vb.CanHitTarget(x), 0f, 9999f);
								if (thing != null){
									Job job2 = JobMaker.MakeJob(JobDefOf.UseVerbOnThing);
									job2.verbToUse = vb;
									job2.targetA = thing;
									job2.maxNumStaticAttacks = 1000000;
									job2.endIfCantShootInMelee = true;
									(parent as Pawn).jobs.TryTakeOrderedJob(job2, JobTag.Misc);
								}else{
									//Log.Warning("No target");
								}
							}/**else if(vd.noTarget){
								ExecuteStackContext.SA_StaticContext.clear();
								ExecuteStackContext.SA_StaticContext.verbScript = vd.ai_targetPoints;
								ExecuteStackContext.SA_StaticContext.thingVariableHolder = variableHolder;
								TargetAndMode obj = ExecuteStackContext.SA_StaticContext.tryExecuteEnum0Delay(parent).singular().recast<TargetAndMode>();
								//Log.Warning(obj.ToString());
								switch(obj.targetMode){
									case -1.0f: {
										break;
									}
									case 0.0f:
									case 1.0f: {
										Job job2 = JobMaker.MakeJob(JobDefOf.UseVerbOnThing);
										job2.verbToUse = vb;
										job2.targetA = obj.targetInfo;
										job2.maxNumStaticAttacks = 1000000;
										job2.endIfCantShootInMelee = true;
										(parent as Pawn).jobs.TryTakeOrderedJob(job2, JobTag.Misc);
										break;
									}
								}
							}**/else{
								ExecuteStackContext.SA_StaticContext.clear();
								ExecuteStackContext.SA_StaticContext.verbScript = vd.ai_targetPoints;
								ExecuteStackContext.SA_StaticContext.thingVariableHolder = variableHolder;
								TargetAndMode obj = ExecuteStackContext.SA_StaticContext.tryExecuteEnum0Delay(parent).singular().recast<TargetAndMode>();
								//Log.Warning(obj.ToString());
								switch(obj.targetMode){
									case -1.0f: {
										break;
									}
									/**case 0.0f: {
										Log.Warning("Attack that thing");
										Job job2 = JobMaker.MakeJob(AVVerbDefOf.UseVerbOnThingContinuous);
										job2.verbToUse = vb;
										job2.targetA = obj.targetInfo;
										job2.maxNumStaticAttacks = 1000000;
										job2.endIfCantShootInMelee = true;
										(parent as Pawn).jobs.TryTakeOrderedJob(job2, JobTag.Misc);
										break;
									}**/
									case 0.0f:
									case 1.0f: {
										Job job2 = JobMaker.MakeJob(JobDefOf.UseVerbOnThing);
										job2.verbToUse = vb;
										job2.targetA = obj.targetInfo;
										job2.maxNumStaticAttacks = 1000000;
										job2.endIfCantShootInMelee = true;
										(parent as Pawn).jobs.TryTakeOrderedJob(job2, JobTag.Misc);
										break;
									}
								}
							}

						}

					}
				}
			}

			this.verbTracker.VerbsTick();
        }

        public void tryRefreshVerbs(){
			//Log.Warning("parent " + parent);
			//root.logTest();
            needsRefreshVerbs = false;
			SA_VerbDataSet.Clear();
			SA_VerbAddNew.Clear();
			SA_VerbDataSet.AddRange(this.root.allMatch(parent as Pawn));
			foreach(VerbData verbD in SA_VerbDataSet){
				if(!verbDataToVerb.ContainsKey(verbD)){
					SA_VerbAddNew.Add(verbD);
					Verb verC = CreateVerb(verbD);
					verbDataToVerb.Add(verbD, verC);
					verbToVerbData.Add(verC, verbD);
					lastUsedTicks.setSafe(verbD, Find.TickManager.TicksGame - verbD.cooldownTicks + verbD.initialCooldownTicks);
				}
			}

			verbDataToVerb.RemoveAll(x => !SA_VerbDataSet.Contains(x.Key) && verbToVerbData.Remove(x.Value));
			this.aiVerbChecks.Clear();
			foreach(VerbData vd in verbDataToVerb.Keys){
				if(vd.AICanEverUse){
					aiVerbChecks.Add(vd);
				}
			}
			/**verbToVerbData.Clear();
			foreach(KeyValuePair<VerbData, Verb> vdv in verbDataToVerb){
				verbToVerbData.Add(vdv.Value, vdv.Key);
			}**/
			verbSync.Clear();
			verbSync.AddRange(verbDataToVerb.Values);

        }
		public override void PostExposeData(){
			base.PostExposeData();
			Scribe_Deep.Look<VariableHolder>(ref variableHolder, "variableHolder");
			Scribe_Collections.Look<ExecuteStackContext>(ref activeExecuteStacks, "activeExecuteStacks", LookMode.Deep);
			Scribe_Collections.Look<int, LocalTargetInfoListed>(ref IDtoExtraTargets, "IDtoExtraTargets", LookMode.Value, LookMode.Deep);
		}
		
		public static HashSet<VerbData> SA_VerbDataSet = new HashSet<VerbData>();
		public static HashSet<VerbData> SA_VerbAddNew = new HashSet<VerbData>();

		public List<ExecuteStackContext> activeExecuteStacks = new List<ExecuteStackContext>();

		public VariableHolder variableHolder = new VariableHolder();

        public JobDef lastJobDef;
        public bool needsRefreshVerbs = true;
		public Dictionary<VerbData, int> lastUsedTicks = new Dictionary<VerbData, int>();
		public int cooldownLeft(VerbData vd){
			return Mathf.Max(0, lastUsedTicks.getSafe(vd) + vd.cooldownTicks - Find.TickManager.TicksGame);
		}

		public static string FailReason(Verb verb, VerbData vd, Pawn pawn, Comp_VerbHolder vholder, bool checkingForAI){
			if(!checkingForAI && !pawn.IsColonistPlayerControlled){
				return "VerbScript.Reason.CantControl";
			}
			if(!checkingForAI && !pawn.Drafted){
				return "VerbScript.Reason.NotDrafted";
			}
			if(vd.allow != null){
				ExecuteStackContext.SA_StaticContext.clear();
				ExecuteStackContext.SA_StaticContext.verbScript = vd.allow;//vd.ai_targetPoints;
				ExecuteStackContext.SA_StaticContext.thingVariableHolder = vholder.variableHolder;
				//Log.Warning(ExecuteStackContext.SA_StaticContext.tryExecuteEnum0Delay(caster).singular());
				bool ff = ExecuteStackContext.SA_StaticContext.tryExecuteEnum0Delay(pawn).singular().recast<bool>();
				if(!ff){
					return "VerbScript.Reason.NotAllowed";
				}
			}
			if(pawn.Downed){
				return "VerbScript.Reason.Downed";
			}
			if(vd.isViolent && (pawn.story.DisabledWorkTagsBackstoryAndTraits & WorkTags.Violent) != 0){
				return "VerbScript.Reason.IncapableOfViolence";
			}
			if(vholder.cooldownLeft(vd) > 0){
				return "VerbScript.Reason.Cooldown";
			}
			return null;
		}

		public Dictionary<VerbData, Verb> verbDataToVerb = new Dictionary<VerbData, Verb>();
		public Dictionary<Verb, VerbData> verbToVerbData = new Dictionary<Verb, VerbData>();
		public List<Verb> verbSync = new List<Verb>();
		public List<VerbData> aiVerbChecks = new List<VerbData>();
		public Dictionary<int, LocalTargetInfoListed> IDtoExtraTargets = new Dictionary<int, LocalTargetInfoListed>();
    }
	
    public class LocalTargetInfoListed : IExposable {
		public List<LocalTargetInfoElement> elements = new List<LocalTargetInfoElement>();
        public virtual void ExposeData() {
			Scribe_Collections.Look<LocalTargetInfoElement>(ref elements, "elements", LookMode.Deep);
        }

	}
    public struct LocalTargetInfoElement : IExposable {
		public LocalTargetInfo localTargetInfo;
        public void ExposeData() {
			Scribe_TargetInfo.Look(ref localTargetInfo, "innerLocalTargetInfo");
        }
    }

}
