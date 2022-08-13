using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VerbScript{
	public class Verb_Scripted : Verb_LaunchProjectile{
		public static List<IntVec3> cellIterator = new List<IntVec3>();
		public override void DrawHighlight(LocalTargetInfo target){
			this.verbProps.DrawRadiusRing(this.caster.Position);
			if (target.IsValid && CanHitTarget(target)){
				GenDraw.DrawTargetHighlight(target);
				Comp_VerbHolder cva = caster.TryGetComp<Comp_VerbHolder>();
				if(cva.verbToVerbData.TryGetValue(this, out VerbData vdd)){
					if(vdd.cellHighlight != null){
						ExecuteStackContext.SA_StaticContext.clear();
						ExecuteStackContext.SA_StaticContext.verbScript = vdd.cellHighlight;
						ExecuteStackContext.SA_StaticContext.Param_TargetInfo = target;
						ExecuteStackContext.SA_StaticContext.thingVariableHolder = cva.variableHolder;
						cellIterator.Clear();
						foreach(object obj in ExecuteStackContext.SA_StaticContext.tryExecuteEnum0Delay(this.caster)){
							cellIterator.Add(obj.recast<IntVec3>());
							//Log.Warning(obj + "");
						}
						GenDraw.DrawFieldEdges(cellIterator);
						cellIterator.Clear();
					}
				}
				//this.DrawHighlightFieldRadiusAroundTarget(target);
			}
		}
		public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter){
			needLOSToCenter = true;
			ThingDef projectile = this.Projectile;
			if (projectile == null)
			{
				return 0f;
			}
			return projectile.projectile.explosionRadius;
		}
		public override bool CanHitTarget(LocalTargetInfo target){
			if(base.CanHitTarget(target)){
				Comp_VerbHolder cva = caster.TryGetComp<Comp_VerbHolder>();
				if(cva.verbToVerbData.TryGetValue(this, out VerbData vdd)){
					if(vdd.allowTarget == null){
						return true;
					}else{
						ExecuteStackContext.SA_StaticContext.clear();
						ExecuteStackContext.SA_StaticContext.verbScript = vdd.allowTarget;//vd.ai_targetPoints;
						ExecuteStackContext.SA_StaticContext.thingVariableHolder = cva.variableHolder;
						//Log.Warning(ExecuteStackContext.SA_StaticContext.tryExecuteEnum0Delay(caster).singular());
						bool ff = ExecuteStackContext.SA_StaticContext.tryExecuteEnum0Delay(target).singular().recast<bool>();
						if(ff){
							return true;
						}
					}
				}
			}
			return false;
		}

		public override void OnGUI(LocalTargetInfo target){
			if (this.CanHitTarget(target) && this.verbProps.targetParams.CanTarget(target.ToTargetInfo(this.caster.Map))){
				Comp_VerbHolder cva = caster.TryGetComp<Comp_VerbHolder>();
				if(cva.verbToVerbData.TryGetValue(this, out VerbData vdd)){
					if(vdd.allowTarget == null){
						base.OnGUI(target);
						return;
					}else{
						ExecuteStackContext.SA_StaticContext.clear();
						ExecuteStackContext.SA_StaticContext.verbScript = vdd.allowTarget;//vd.ai_targetPoints;
						ExecuteStackContext.SA_StaticContext.thingVariableHolder = cva.variableHolder;
						//Log.Warning(ExecuteStackContext.SA_StaticContext.tryExecuteEnum0Delay(caster).singular());
						bool ff = ExecuteStackContext.SA_StaticContext.tryExecuteEnum0Delay(target).singular().recast<bool>();
						if(ff){
							base.OnGUI(target);
							return;
						}
					}
				}
			}
			GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
		}
		protected override bool TryCastShot(){
			Comp_VerbHolder cva = caster.TryGetComp<Comp_VerbHolder>();
			if(cva.verbToVerbData.TryGetValue(this, out VerbData vdd)){
				/**ExecuteStackContext newContext = ExecuteStackContext.nextScriptExecutorContext();
				newContext.verbScript = vdd.fire;
				
				newContext.Param_TargetInfo = this.CurrentTarget;
				cva.activeExecuteStacks.Add(newContext);**/
			}
			return true;
		}

		public override bool Available(){
			/**if (!base.Available())
			{
				return false;
			}**/
			if (this.CasterIsPawn)
			{
				Pawn casterPawn = this.CasterPawn;
				if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
				{
					return false;
				}
			}
			return true;
		}
	}
}
