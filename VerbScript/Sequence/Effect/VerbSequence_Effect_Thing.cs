using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbScript {
    public class VE_MentalState : VerbEffect {
		[FixedLoad]
        public VerbSequence mentalState;
		[FixedLoad]
        public VerbSequence target;
        public override void RegisterAllTypes(VerbRootQD destination){
            mentalState.RegisterAllTypes(destination);
            target.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            mentalState = mentalState.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            target = target.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            mentalState.appendID();
            target.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            MentalStateDef state = Recast.recast<MentalStateDef>(mentalState.quickEvaluate(context).singular());
            Pawn thing = Recast.recast<Thing>(target.quickEvaluate(context).singular()) as Pawn;
            thing.mindState.mentalStateHandler.TryStartMentalState(state);
            yield break;
        }
    }

    public class VE_SetFaction : VerbEffect {
		[FixedLoad][IndexedLoad(0)]
        public VerbSequence setFactionOf;
		[FixedLoad][IndexedLoad(1)]
        public VerbSequence copyFactionFrom;
        public override void RegisterAllTypes(VerbRootQD destination){
            copyFactionFrom.RegisterAllTypes(destination);
            setFactionOf.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            copyFactionFrom = copyFactionFrom.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            setFactionOf = setFactionOf.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            copyFactionFrom.appendID();
            setFactionOf.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Thing copy = Recast.recast<Thing>(copyFactionFrom.quickEvaluate(context).singular());
            Thing paste = Recast.recast<Thing>(setFactionOf.quickEvaluate(context).singular());
            paste.SetFaction(copy.Faction);
            yield break;
        }
    }
    public class VE_GiveHediff : VerbEffect {
        [FixedLoad][DefaultType(typeof(VE_HediffDef))][IndexedLoad(0)]
        public VerbSequence hediff;
		[FixedLoad][DefaultType(typeof(VE_Number))]
        public VerbSequence severity = new VE_Number() { number = 1.0f };
		[FixedLoad][DefaultType(typeof(VE_BodyPartDef))]
        public VerbSequence bodypart;
		[FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
		[FixedLoad][DefaultType(typeof(bool))]
        public bool adjustExisting;

        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + adjustExisting + "]");
            SA_StringBuilder.Append("[");
            bodypart?.appendID();
            severity.appendID();
            hediff.appendID();
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override void RegisterAllTypes(VerbRootQD destination){
            bodypart?.RegisterAllTypes(destination);
            severity.RegisterAllTypes(destination);
            hediff.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            bodypart = bodypart?.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            severity = severity.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            hediff = hediff.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            Pawn pawn = Recast.recast<Thing>(targetScope.quickEvaluate(context).singular()) as Pawn;
            HediffDef hdToApply = Recast.recast<HediffDef>(hediff.quickEvaluate(context).singular());
            BodyPartDef bodyPartDef = bodypart == null? null : Recast.recast<BodyPartDef>(bodypart.quickEvaluate(context).singular());
            float sever = Recast.recast<float>(severity.quickEvaluate(context).singular());

            if(adjustExisting){
                Hediff hd = pawn.health.hediffSet.hediffs.Find(x => x.def == hdToApply && ((x.Part == null && bodyPartDef == null) || (x.Part != null && x.Part.def == bodyPartDef)));
                if(hd == null){
			        Hediff hediff = HediffMaker.MakeHediff(hdToApply, pawn, null);
			        hediff.Severity = sever;
			        pawn.health.AddHediff(hediff, pawn.RaceProps.body.AllParts.Find((BodyPartRecord x) => x.def == bodyPartDef), null, null);
                }else{
                    hd.Severity += sever;
                }
            }else{
			    Hediff hediff = HediffMaker.MakeHediff(hdToApply, pawn, null);
			    hediff.Severity = sever;
			    pawn.health.AddHediff(hediff, pawn.RaceProps.body.AllParts.Find((BodyPartRecord x) => x.def == bodyPartDef), null, null);
            }

            yield break;
        }

    }
    
    public class VE_AddTrait : VerbEffect {
		[FixedLoad][DefaultType(typeof(TraitDef))]
        public TraitDef traitDef;
		[FixedLoad][DefaultType(typeof(int))]
        public int degree;
		[FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        [FixedLoad][DefaultType(typeof(bool))]
        public bool overrideConflictingTraits = true;

        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + traitDef + degree + overrideConflictingTraits + "]");
            SA_StringBuilder.Append("[");
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override void RegisterAllTypes(VerbRootQD destination){
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            Pawn pawn = Recast.recast<Thing>(targetScope.quickEvaluate(context).singular()) as Pawn;
            if(pawn.story != null){
                if(!pawn.story.traits.HasTrait(traitDef)){
                    pawn.story.traits.GainTrait(new Trait(traitDef, degree));
                }else if(overrideConflictingTraits){
                    if(pawn.story.traits.DegreeOfTrait(traitDef) != degree){
                        Harmony_VerbScriptHook.RemoveTrait(pawn, traitDef);
                        pawn.story.traits.GainTrait(new Trait(traitDef, degree));
                    }
                }
            }
            yield break;
        }
    }
    public class VE_RemoveTrait : VerbEffect {
		[FixedLoad][DefaultType(typeof(TraitDef))]
        public TraitDef traitDef;
		[FixedLoad][DefaultType(typeof(int))]
        public int degree;
		[FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();

        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + traitDef + degree + "]");
            SA_StringBuilder.Append("[");
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override void RegisterAllTypes(VerbRootQD destination){
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            Pawn pawn = Recast.recast<Thing>(targetScope.quickEvaluate(context).singular()) as Pawn;
            if(pawn.story != null){
                if(pawn.story.traits.HasTrait(traitDef) && pawn.story.traits.DegreeOfTrait(traitDef) == degree){
                    Harmony_VerbScriptHook.RemoveTrait(pawn, traitDef);
                }
            }
            yield break;
        }
    }
    public class VE_Train : VerbEffect {
        [FixedLoad][DefaultType(typeof(VE_TrainableDef))][IndexedLoad(0)]
        public VerbSequence trainable;
		[FixedLoad][DefaultType(typeof(VE_Number))]
        public VerbSequence stages = new VE_Number() { number = 1.0f };
		[FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
		[FixedLoad]
        public VerbSequence trainerScope = new VS_RootScope();

        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            trainable.appendID();
            stages.appendID();
            targetScope.appendID();
            trainerScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override void RegisterAllTypes(VerbRootQD destination){
            trainable.RegisterAllTypes(destination);
            stages.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            trainerScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            trainable = trainable.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            trainerScope = trainerScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            stages = stages.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            Pawn pawn = Recast.recast<Thing>(targetScope.quickEvaluate(context).singular()) as Pawn;
            Pawn pawntr = Recast.recast<Thing>(trainerScope.quickEvaluate(context).singular()) as Pawn;
            if(pawn.training != null){
                TrainableDef rd = Recast.recast<TrainableDef>(trainable.quickEvaluate(context).singular());
                if(pawn.training.CanBeTrained(rd)){
                    float iter = Recast.recast<float>(stages.quickEvaluate(context).singular());
                    for(int i = 0; i < iter; i++){
                        pawn.training.Train(rd, pawntr);
                    }
                }
            }
            yield break;
        }
    }
    
    public class VE_Teleport : VerbEffect {
		[FixedLoad][IndexedLoad(1)]
        public VerbSequence destination;
		[FixedLoad][IndexedLoad(0)]
        public VerbSequence targetScope;
        public override void RegisterAllTypes(VerbRootQD destination){
            this.destination.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            destination = destination.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            destination.appendID();
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            IntVec3 dest = Recast.recast<IntVec3>(destination.quickEvaluate(context).singular());
            Pawn paste = Recast.recast<Pawn>(targetScope.quickEvaluate(context).singular());
			paste.pather.StopDead();
			paste.Position = dest;
			paste.Notify_Teleported(true, true);
            yield break;
        }
    }

}
