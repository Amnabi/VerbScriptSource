using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using System.Xml;

namespace VerbScript {
    public class VC_CurJobDef : VC_SingleScope{
		[FixedLoad][DefaultType(typeof(JobDef))][DirectLoad]
        public JobDef jobDef = JobDefOf.Wait;
        public override int uniqueSubIDFromContent(){
            return jobDef.index;
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(jobDef.index);
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            JobDef curJob = (targetScope.quickEvaluate(context).singular().recast<Pawn>()).CurJobDef;
            yield return curJob == null? -1 : curJob.index;
        }
    }
    public class VC_HasEquipment : VC_SingleScope{
		[FixedLoad][DefaultType(typeof(ThingDef))][DirectLoad]
        public ThingDef thingDef;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + thingDef.defName + "]");
        }
        public override int uniqueSubIDFromContent(){
            return thingDef.index;
        }
        public override ICollection<object> CreateResultCacheCollection(){
            return FreePool<HashSet<object>>.next();
        }
        public static HashSet<int> SA_EnsureUniqueReturn = new HashSet<int>();
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            SA_EnsureUniqueReturn.Clear();
            Pawn pawn = targetScope.quickEvaluate(context).singular().recast<Pawn>();
            foreach(Thing t in pawn.equipment?.AllEquipmentListForReading){
                int iss2 = t.def.index;
                if(SA_EnsureUniqueReturn.Add(iss2)){
                    yield return iss2;
                }
            }
            SA_EnsureUniqueReturn.Clear();
        }
    }
    public class VC_HasHediff : VC_SingleScope{
		[FixedLoad][DefaultType(typeof(HediffDef))][DirectLoad]
        public HediffDef hediffDef;
		[FixedLoad][DefaultType(typeof(BodyPartDef))]
        public BodyPartDef bodyPartDef;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + hediffDef.defName + (bodyPartDef == null? "" : ("/" + bodyPartDef.defName)) + "]");
        }
        public override int uniqueSubIDFromContent(){
            return hediffDef.index | (bodyPartDef == null? 0x7FFF0000 : (bodyPartDef.index << 16)); //return 0;
        }
        public override ICollection<object> CreateResultCacheCollection(){
            return FreePool<HashSet<object>>.next();
        }
        /**public override int QD_quickFilterIDInt(){
            return hediffDef.index;
        }**/
        public static HashSet<int> SA_EnsureUniqueReturn = new HashSet<int>();
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            SA_EnsureUniqueReturn.Clear();
            Pawn pawn = targetScope.quickEvaluate(context).singular().recast<Pawn>();
            foreach(Hediff hd in pawn.health.hediffSet.hediffs){
                if(hd.Part != null){
                    int iss = (hd.Part.def.index << 16) | hd.def.index;
                    if(SA_EnsureUniqueReturn.Add(iss)){
                        //Log.Warning("Yield return " + iss);
                        yield return iss;
                    }
                }
                int iss2 = (0x7FFF0000) | hd.def.index;
                if(SA_EnsureUniqueReturn.Add(iss2)){
                    //Log.Warning("Yield return " + iss2);
                    yield return iss2;
                }
                //Log.Warning("WHUY " + pawn + " / " + hd.def.defName);
            }
            SA_EnsureUniqueReturn.Clear();
        }

        /**public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return ((targetScope.quickEvaluate(context).singular().recast<Pawn>()).health.hediffSet.HasHediff(hediffDef))? uniqueSubIDFromContent() : -1;
        }**/
    }
    public class VC_HasTrait : VC_SingleScope{
        [DirectLoad]
        public static Action<VerbSequence, XmlNode, string> CUSTOMLOADER = (verbSeq, node, str) => {
            str = str.Replace(" ", "");
            string[] strs = str.Split(',');
            VC_HasTrait vs3 = (VC_HasTrait)verbSeq;
            if(strs.Length == 1){
                vs3.degree = 0;
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(vs3, "traitDef", strs[0], null, null);
            }else{
                vs3.degree = ParseHelper.FromString<int>(strs[1]);
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(vs3, "traitDef", strs[0], null, null);
            }
        };
		[FixedLoad][DefaultType(typeof(TraitDef))]
        public TraitDef traitDef;
		[FixedLoad][DefaultType(typeof(int))]
        public int degree;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + traitDef.defName + degree + "]");
        }
        public override long PG_groupIndex(){
            return traitDef.index;
        }
        public override int uniqueSubIDFromContent(){
            return degree + 256;
        }
        public override ICollection<object> CreateResultCacheCollection(){
            return FreePool<HashSet<object>>.next();
        }
        public override int QD_quickFilterIDInt(){
            return (traitDef.index << 16) | (degree + 256);
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Pawn pawn = (targetScope.quickEvaluate(context).singular().recast<Pawn>());
            yield return pawn.story.traits.HasTrait(traitDef)? (pawn.story.traits.DegreeOfTrait(traitDef) + 256) : -1;
        }
    }
    public class VC_Drafted : VC_SingleScope{
		[FixedLoad][DefaultType(typeof(bool))][DirectLoad]
        public bool draftTrue = true;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + draftTrue + "]");
        }
        public override int uniqueSubIDFromContent(){
            return draftTrue ? 1 : 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Pawn pawn = (targetScope.quickEvaluate(context).singular().recast<Pawn>());
            yield return (pawn.Drafted == draftTrue)? uniqueSubIDFromContent() : -1;
        }
    }
    public class VC_IsAnimal : VC_SingleScope{
        public override void appendID(){
            base.appendID();
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Pawn pawn = (targetScope.quickEvaluate(context).singular().recast<Pawn>());
            yield return pawn != null && pawn.RaceProps.Animal? 0 : -1;
        }
    }
    public class VC_IsMechanoid : VC_SingleScope{
        public override void appendID(){
            base.appendID();
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Pawn pawn = (targetScope.quickEvaluate(context).singular().recast<Pawn>());
            yield return pawn != null && pawn.RaceProps.IsMechanoid? 0 : -1;
        }
    }
    public class VC_IsHumanlike : VC_SingleScope{
        public override void appendID(){
            base.appendID();
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Pawn pawn = (targetScope.quickEvaluate(context).singular().recast<Pawn>());
            yield return pawn != null && pawn.RaceProps.Humanlike? 0 : -1;
        }
    }
    public class VC_Moving : VC_SingleScope{
		[FixedLoad][DefaultType(typeof(bool))][DirectLoad]
        public bool movingTrue = true;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + movingTrue + "]");
        }
        public override int uniqueSubIDFromContent(){
            return movingTrue ? 1 : 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Pawn pawn = (targetScope.quickEvaluate(context).singular().recast<Pawn>());
            yield return (pawn.pather != null && pawn.pather.Moving == movingTrue)? uniqueSubIDFromContent() : -1;
        }
    }
    public class VC_HediffDef : VC_SingleScope{
		[FixedLoad][DefaultType(typeof(HediffDef))][DirectLoad]
        public HediffDef def = HediffDefOf.AlcoholHigh;
        //public static Dictionary<int, ThingDef> SA_IndexToDef = new Dictionary<int, ThingDef>();
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + def.defName + "]");
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            /**if(!SA_IndexToDef.ContainsKey(def.index)){
                SA_IndexToDef.Add(def.index, def);
            }**/
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override int uniqueSubIDFromContent(){
            return def.index;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Hediff hediff = (targetScope.quickEvaluate(context).singular().recast<Hediff>());
            yield return hediff.def == null? -1 : hediff.def.index;
        }
    }
    public class VC_ThingDef : VC_SingleScope{
		[FixedLoad][DefaultType(typeof(ThingDef))][DirectLoad]
        public ThingDef def = ThingDefOf.Human;
        public static Dictionary<int, ThingDef> SA_IndexToDef = new Dictionary<int, ThingDef>();
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + def.defName + "]");
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            if(!SA_IndexToDef.ContainsKey(def.index)){
                SA_IndexToDef.Add(def.index, def);
            }
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override int uniqueSubIDFromContent(){
            return def.index;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Thing pawn = (targetScope.quickEvaluate(context).singular().recast<Thing>());
            yield return pawn.def == null? -1 : pawn.def.index;
        }
    }
    public class VC_Gender : VC_SingleScope{
		[FixedLoad][DefaultType(typeof(Gender))][DirectLoad]
        public Gender gender;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + gender.ToString() + "]");
        }
        public override int uniqueSubIDFromContent(){
            return (int)gender;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Pawn pawn = (targetScope.quickEvaluate(context).singular().recast<Thing>()) as Pawn;
            yield return (int)pawn.gender;
        }
    }
    public class VC_InMentalState : VC_SingleScope{
        [DirectLoad]
        public static Action<VerbSequence, XmlNode, string> CUSTOMLOADER = (verbSeq, node, str) => {
            VC_InMentalState vs3 = (VC_InMentalState)verbSeq;
            if(!str.NullOrEmpty()){
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(vs3, "mentalStateDef", str, null, null);
            }
        };
		[FixedLoad][DefaultType(typeof(MentalStateDef))]
        public MentalStateDef mentalStateDef;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + (mentalStateDef == null? "BE1AAF0F_NULL" : mentalStateDef.defName ) + "]");
        }
        public override int uniqueSubIDFromContent(){
            return mentalStateDef == null? 65536 : mentalStateDef.index;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Pawn pawn = (targetScope.quickEvaluate(context).singular().recast<Thing>()) as Pawn;
            MentalStateDef msDef = pawn.MentalStateDef;
            if(mentalStateDef == null){
                yield return msDef != null? 65536 : -1;
                yield break;
            }
            yield return msDef == null? -1 : msDef.index;
        }
    }
    public class VC_LineOfSight : VC_SingleScope{
		[FixedLoad]
        public VerbSequence casterScope = new VS_TopScope();
        public override void RegisterAllTypes(VerbRootQD destination){
            casterScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            casterScope = casterScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            casterScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Thing targetT = (targetScope.quickEvaluate(context).singular().recast<Thing>());
            IntVec3 target = targetT.recast<IntVec3>();
            Map targetMap = targetT.recast<Map>();
            IntVec3 caster = (casterScope.quickEvaluate(context).singular().recast<IntVec3>());
            yield return GenSight.LineOfSight(caster, target, targetMap)? 0 : -1;
        }
    }
}
