using System;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;
using Verse;

namespace VerbScript{
	public class JobDriver_AttackStaticSingleVerb : JobDriver{
		public override void ExposeData(){
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.startedIncapacitated, "startedIncapacitated", false, false);
			Scribe_Values.Look<int>(ref this.numAttacksMade, "numAttacksMade", 0, false);
		}
		public override bool TryMakePreToilReservations(bool errorOnFailed){
			return true;
		}
		public Verb getCurrentPawnVerb{
			get{
				Pawn p = pawn;
				if(p.stances != null && p.stances.curStance is Stance_Busy bz){
					return bz.verb;
				}
				return null;
			}
		}

		public bool canUseVerb(Verb verb){
			Comp_VerbHolder cvh = this.pawn.TryGetComp<Comp_VerbHolder>();
			return Comp_VerbHolder.FailReason(verb, cvh.verbToVerbData[verb], pawn, cvh, false) == null;
		}
		protected override IEnumerable<Toil> MakeNewToils(){
			yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
			Toil init = new Toil();
			init.initAction = delegate(){
				Pawn pawn = this.TargetThingA as Pawn;
				if (pawn != null){
					this.startedIncapacitated = pawn.Downed;
				}
				this.pawn.pather.StopDead();
			};
			init.tickAction = delegate(){
				if (!this.TargetA.IsValid){
					this.EndJobWith(JobCondition.Succeeded);
					return;
				}
				if (this.TargetA.HasThing){
					Pawn pawn = this.TargetA.Thing as Pawn;
					if (this.TargetA.Thing.Destroyed || (pawn != null && !this.startedIncapacitated && pawn.Downed) || (pawn != null && pawn.IsInvisible())){
						this.EndJobWith(JobCondition.Succeeded);
						return;
					}
				}
				if (this.numAttacksMade >= this.job.maxNumStaticAttacks && !this.pawn.stances.FullBodyBusy){
					this.EndJobWith(JobCondition.Succeeded);
					return;
				}
				if (getCurrentPawnVerb != this.job.verbToUse && Find.TickManager.TicksGame % 10 == 0 && canUseVerb(job.verbToUse) && job.verbToUse.TryStartCastOn(this.job.GetTarget(TargetIndex.A), false, true)){
					this.numAttacksMade++;
					return;
				}
				if (!this.pawn.stances.FullBodyBusy){
					Verb verb = this.job.verbToUse;//this.pawn.TryGetAttackVerb(this.TargetA.Thing, !this.pawn.IsColonist);
					if (this.job.endIfCantShootTargetFromCurPos && (verb == null || !verb.CanHitTargetFrom(this.pawn.Position, this.TargetA))){
						this.EndJobWith(JobCondition.Incompletable);
						return;
					}
					if (this.job.endIfCantShootInMelee){
						if (verb == null){
							this.EndJobWith(JobCondition.Incompletable);
							return;
						}
						float num = verb.verbProps.EffectiveMinRange(this.TargetA, this.pawn);
						if ((this.TargetA.Pawn != this.pawn) && (float)this.pawn.Position.DistanceToSquared(this.TargetA.Cell) < num * num && this.pawn.Position.AdjacentTo8WayOrInside(this.TargetA.Cell)){
							this.EndJobWith(JobCondition.Incompletable);
							return;
						}
					}
				}
			};
			init.defaultCompleteMode = ToilCompleteMode.Never;
			init.activeSkill = (() => Toils_Combat.GetActiveSkillForToil(init));
			yield return init;
			yield break;
		}

		//public Verb verb;
		private bool startedIncapacitated;
		private int numAttacksMade;
	}
}
