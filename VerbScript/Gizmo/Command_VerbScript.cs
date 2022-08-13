using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VerbScript {
	public static class VerbTargetUtility{
		public static bool isSelfOnly(VerbProperties verbProp){
			TargetingParameters tP = verbProp.targetParams;
			if(tP.canTargetSelf){
				return !tP.canTargetLocations && !tP.canTargetBuildings && !tP.canTargetPawns && !tP.canTargetItems;
			}
			return false;
		}
	}

	[StaticConstructorOnStartup]
	public class Command_VerbScript : Command
	{
		public override Color IconDrawColor
		{
			get
			{
				if (this.verb.EquipmentSource != null)
				{
					return this.verb.EquipmentSource.DrawColor;
				}
				return base.IconDrawColor;
			}
		}

		public override void GizmoUpdateOnMouseover()
		{
			if (!this.drawRadius)
			{
				return;
			}
			this.verb.verbProps.DrawRadiusRing(this.verb.caster.Position);
			if (!this.groupedVerbs.NullOrEmpty<Verb>())
			{
				foreach (Verb verb in this.groupedVerbs)
				{
					verb.verbProps.DrawRadiusRing(verb.caster.Position);
				}
			}
		}

		public override void MergeWith(Gizmo other)
		{
			base.MergeWith(other);
			Command_VerbScript command_VerbScript = other as Command_VerbScript;
			if (command_VerbScript == null)
			{
				Log.ErrorOnce("Tried to merge Command_VerbTarget with unexpected type", 73406263, false);
				return;
			}
			if (this.groupedVerbs == null)
			{
				this.groupedVerbs = new List<Verb>();
			}
			this.groupedVerbs.Add(command_VerbScript.verb);
			if (command_VerbScript.groupedVerbs != null)
			{
				this.groupedVerbs.AddRange(command_VerbScript.groupedVerbs);
			}
		}
		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth);
			int ticksLeft = verbHolder.cooldownLeft(verbData);
			if (ticksLeft > 0)
			{
				float num = Mathf.InverseLerp(verbData.cooldownTicks, 0f, ticksLeft);
				Widgets.FillableBar(rect, Mathf.Clamp01(num), cooldownBarTex, null, false);
				if(ticksLeft > 0){
					Text.Font = GameFont.Tiny;
					Text.Anchor = TextAnchor.UpperCenter;
					Widgets.Label(rect, ticksLeft.ToStringSecondsFromTicks());
					Text.Anchor = TextAnchor.UpperLeft;
				}
			}
			if (result.State == GizmoState.Interacted)
			{
				return result;
			}
			return new GizmoResult(result.State);
		}
		public override void ProcessInput(Event ev){
			base.ProcessInput(ev);
			/**ExecuteStackContext newContext = ExecuteStackContext.nextScriptExecutorContext();
			newContext.verbScript = verbData.init;
			verbHolder.activeExecuteStacks.Add(newContext);**/
		}

		public Comp_VerbHolder verbHolder;
		public VerbData verbData;
		public Verb verb;
		protected List<Verb> groupedVerbs;
		public bool drawRadius = true;

		public static Verb SA_KeyReference;
		public static Dictionary<Pawn, Command_VerbScript> SA_OneToOne = new Dictionary<Pawn, Command_VerbScript>();
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(203, 203, 203, 64));
	}
	public class Command_VerbScriptTarget : Command_VerbScript
	{
		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
			Targeter targeter = Find.Targeter;
			if (this.verb.CasterIsPawn && targeter.targetingSource != null && targeter.targetingSource.GetVerb.verbProps == this.verb.verbProps)
			{
				Pawn casterPawn = this.verb.CasterPawn;
				if (!targeter.IsPawnTargeting(casterPawn))
				{
					targeter.targetingSourceAdditionalPawns.Add(casterPawn);
					SA_OneToOne.Add(casterPawn, this);
					return;
				}
			}
			else
			{
				SA_OneToOne.Clear();
				Pawn casterPawn = this.verb.CasterPawn;
				//SA_OneToOne.Add(casterPawn, this);
				SA_KeyReference = this.verb;
				Find.Targeter.BeginTargeting(this.verb, null);
			}
		}
	}
	public class Command_VerbScriptNonTarget : Command_VerbScript{
		public override void ProcessInput(Event ev){
			base.ProcessInput(ev);
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
			Targeter targeter = Find.Targeter;
			
			Job job2 = JobMaker.MakeJob(AVVerbDefOf.UseVerbOnThingContinuous);
			job2.verbToUse = this.verb;
			job2.targetA = verbHolder.pawn;
            job2.maxNumStaticAttacks = this.verbData.repeatVerb? 10000000 : 1;
			job2.endIfCantShootInMelee = true;
			verbHolder.pawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);
		}
	}
}
