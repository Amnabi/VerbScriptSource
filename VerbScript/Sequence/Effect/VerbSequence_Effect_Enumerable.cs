using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using System.Xml;

namespace VerbScript {
    public class VE_CallScript : VerbEffect {
        private VerbPartialScript private_partialScript;
        public VerbPartialScript script{
            get{
                if(private_partialScript == null){
                    private_partialScript = VerbPartialScript.named(scriptName);
                }
                return private_partialScript;
            }
        }
        [DirectLoad]
        public string scriptName;
        public override int scriptIndexMax(int scriptSequenceGroupSubIndex){
            return script.verbScript.sequence.Count();
        }
        public override VerbSequence getSequenceAt(int i, int subSequenceGroupIndex){
            return script.verbScript.sequence[i];
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(scriptName);
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            object lastObj = null;
            /**foreach(VerbSequence vs in verbSequences){
                foreach(object obj in vs.quickEvaluate(context)){
                    lastObj = obj;
                }
            }**/
            yield return lastObj;
        }
    }
    public class VE_ReturnLast : VerbEffect {
        [ListLoad(typeof(VerbSequence))]
		public List<VerbSequence> verbSequences = new List<VerbSequence>();
        public override void RegisterAllTypes(VerbRootQD destination){
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].RegisterAllTypes(destination);
            }
            base.RegisterAllTypes(destination);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].appendID();
            }
            SA_StringBuilder.Append("]");
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i] = verbSequences[i].registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            }
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            object lastObj = null;
            foreach(VerbSequence vs in verbSequences){
                foreach(object obj in vs.quickEvaluate(context)){
                    lastObj = obj;
                }
            }
            yield return lastObj;
            //yield break;
        }
    }
    public class VE_SelectHighest : VerbEffect {
        [ListLoad(typeof(VerbSequence))]
        public List<VerbSequence> verbSequences = new List<VerbSequence>();
        [FixedLoad]
        public VerbSequence points;
        [FixedLoad] [DefaultType(typeof(VE_Number))]
        public VerbSequence minimum;
        public override void RegisterAllTypes(VerbRootQD destination){
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].RegisterAllTypes(destination);
            }
            points.RegisterAllTypes(destination);
            minimum.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].appendID();
            }
            points.appendID();
            minimum.appendID();
            SA_StringBuilder.Append("]");
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i] = verbSequences[i].registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            }
            points = points.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            if(minimum == null){
                minimum = new VE_Number(){ number = float.MinValue };
            }
            minimum = minimum.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            float valueMax = minimum.quickEvaluate(context).singular().recast<float>();
            object selectedObj = null;
            foreach(VerbSequence vs in verbSequences){
                foreach(object obj in vs.quickEvaluate(context)){
                    context.pushScopeNoPointer(obj);
                    float f = points.quickEvaluate(context).singular().recast<float>();
                    //if(selectedObj == null || f > valueMax){
                    if(f > valueMax){
                        valueMax = f;
                        selectedObj = obj;
                    }
                    context.popScopeNoPointer();
                }
            }
            yield return selectedObj;
        }
    }
    public class VE_SelectRandom : VerbEffect {
        [ListLoad(typeof(VerbSequence))]
        public List<VerbSequence> verbSequences = new List<VerbSequence>();
        public override void RegisterAllTypes(VerbRootQD destination){
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].RegisterAllTypes(destination);
            }
            base.RegisterAllTypes(destination);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].appendID();
            }
            SA_StringBuilder.Append("]");
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i] = verbSequences[i].registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            }
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public static List<object> SA_RandomList = new List<object>();
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            SA_RandomList.Clear();
            foreach(VerbSequence vs in verbSequences){
                foreach(object obj in vs.quickEvaluate(context)){
                    SA_RandomList.Add(obj);
                }
            }
            if(SA_RandomList.Count == 0){
                yield break;
            }
            object selected = SA_RandomList[(int)(Rand.Value * (SA_RandomList.Count))];
            SA_RandomList.Clear();
            yield return selected;
        }
    }
    
    public class VE_Foreach : VerbEffect {
        [ListLoad(typeof(VerbSequence))]
        public List<VerbSequence> verbSequences = new List<VerbSequence>();
        public static Action<VerbSequence, XmlNode, string> TCP_CUSTOMLOADER = (verbSeq, node, str) => {
            VE_Foreach this_ = (VE_Foreach)verbSeq;
            this_.do_ = (VerbSequence)getGenericMethod(typeof(VE_JointSequence)).Invoke(null, new object[] { node, null });
        };
        [FixedLoad(new string[]{ "do" })][CustomLoad(typeof(VE_Foreach), "TCP_CUSTOMLOADER")]
        public VerbSequence do_;
        public override void RegisterAllTypes(VerbRootQD destination){
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].RegisterAllTypes(destination);
            }
            do_.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].appendID();
            }
            do_.appendID();
            SA_StringBuilder.Append("]");
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i] = verbSequences[i].registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            }
            do_ = do_.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            foreach(VerbSequence vs in verbSequences){
                foreach(object obj in vs.quickEvaluate(context)){
                    context.pushScopeNoPointer(obj);
                    do_.quickEvaluate(context).singular();
                    context.popScopeNoPointer();
                    yield return obj;
                }
            }
        }
    }

    public class VE_FilterSequence : VerbEffect {
        [FixedLoad][IndexedLoad(0)]
        public VerbSequence filterCondition;
        [ListLoad(typeof(VerbSequence))]
        public List<VerbSequence> verbSequences = new List<VerbSequence>();
        public override void RegisterAllTypes(VerbRootQD destination){
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].RegisterAllTypes(destination);
            }
            filterCondition.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].appendID();
            }
            filterCondition.appendID();
            SA_StringBuilder.Append("]");
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i] = verbSequences[i].registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            }
            filterCondition = filterCondition.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            foreach(VerbSequence vs in verbSequences){
                foreach(object obj in vs.quickEvaluate(context)){
                    //Log.Warning("Iteration " + obj);
                    context.pushScopeNoPointer(obj);
                    if(filterCondition.quickEvaluateAsBool(context)){
                        yield return obj;
                    }
                    context.popScopeNoPointer();
                }
            }
        }
    }
    
    public class VE_JointSequence : VerbEffect {
        [ListLoad(typeof(VerbSequence))]
        public List<VerbSequence> verbSequences = new List<VerbSequence>();
        public override void RegisterAllTypes(VerbRootQD destination){
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].RegisterAllTypes(destination);
            }
            base.RegisterAllTypes(destination);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].appendID();
            }
            SA_StringBuilder.Append("]");
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i] = verbSequences[i].registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            }
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            foreach(VerbSequence vs in verbSequences){
                foreach(object obj in vs.quickEvaluate(context)){
                    yield return obj;
                }
            }
        }
    }
    
    public class VE_ThingsInRadius : VerbEffect {
        [FixedLoad][DefaultType(typeof(VE_Vector3))][IndexedLoad(0)]
        public VerbSequence position;
		[FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        [FixedLoad][DefaultType(typeof(VE_Number))]
        public VerbSequence radius = new VE_Number(){ number = 0.0f };
        [FixedLoad][DefaultType(typeof(bool))]
        public bool requiresLineOfSight = false;
        public override void RegisterAllTypes(VerbRootQD destination){
            targetScope.RegisterAllTypes(destination);
            position.RegisterAllTypes(destination);
            radius.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            position = position.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            radius = radius.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + requiresLineOfSight + "]");
            SA_StringBuilder.Append("[");
            targetScope.appendID();
            position.appendID();
            radius.appendID();
            SA_StringBuilder.Append("]");
        }

        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Map map = Recast.recast<Map>(targetScope.quickEvaluate(context).singular());
            IntVec3 intVec3 = Recast.recast<IntVec3>(position.quickEvaluate(context).singular());
            float rad = Recast.recast<float>(radius.quickEvaluate(context).singular());
			int num = GenRadial.NumCellsInRadius(rad);
			for (int i = 0; i < num; i++){
				IntVec3 c = intVec3 + GenRadial.RadialPattern[i];
				if (c.InBounds(map) && (!requiresLineOfSight || GenSight.LineOfSight(intVec3, c, map, true))){
                    foreach(Thing t in c.GetThingList(map)){
                        yield return t;
                    }
				}
			}

        }
    }
    public class VE_CellsInRadius : VerbEffect {
        [FixedLoad][DefaultType(typeof(VE_Vector3))][IndexedLoad(0)]
        public VerbSequence position;
		[FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        [FixedLoad][DefaultType(typeof(VE_Number))]
        public VerbSequence radius = new VE_Number(){ number = 0.0f };
        [FixedLoad][DefaultType(typeof(bool))]
        public bool requiresLineOfSight = false;
        public override void RegisterAllTypes(VerbRootQD destination){
            targetScope.RegisterAllTypes(destination);
            position.RegisterAllTypes(destination);
            radius.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            position = position.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            radius = radius.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + requiresLineOfSight + "]");
            SA_StringBuilder.Append("[");
            targetScope.appendID();
            position.appendID();
            radius.appendID();
            SA_StringBuilder.Append("]");
        }

        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Map map = Recast.recast<Map>(targetScope.quickEvaluate(context).singular());
            IntVec3 intVec3 = Recast.recast<IntVec3>(position.quickEvaluate(context).singular());
            float rad = Recast.recast<float>(radius.quickEvaluate(context).singular());
			int num = GenRadial.NumCellsInRadius(rad);
			for (int i = 0; i < num; i++){
				IntVec3 c = intVec3 + GenRadial.RadialPattern[i];
				if (c.InBounds(map) && (!requiresLineOfSight || GenSight.LineOfSight(intVec3, c, map, true))){
                    yield return c;
				}
			}

        }
    }

    /**public class VE_ThingsAtPosition : VerbEffect {
        [FixedLoad][DefaultType(typeof(VE_Vector3))][IndexedLoad(0)]
        public VerbSequence position;
		[FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        public override void RegisterAllTypes(VerbRootQD destination){
            targetScope.RegisterAllTypes(destination);
            position.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            position.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            targetScope.appendID();
            position.appendID();
            SA_StringBuilder.Append("]");
        }

        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Map map = Recast.recast<Map>(targetScope.quickEvaluate(context).singular());
            IntVec3 intVec3 = Recast.recast<IntVec3>(position.quickEvaluate(context).singular());
            if(intVec3.InBounds(map)){
                foreach(Thing target in intVec3.GetThingList(map)){
                    yield return target;
                }
            }
        }
    }**/
    public class VE_HostilePawnsInMap : VerbEffect {
		[FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        public override void RegisterAllTypes(VerbRootQD destination){
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Pawn pawn = Recast.recast<Pawn>(targetScope.quickEvaluate(context).singular());
            Map map = pawn.recast<Map>();
            foreach(IAttackTarget target in map.attackTargetsCache.GetPotentialTargetsFor(pawn)){
                if(target is Pawn){
                    yield return target;
                }
            }
        }
    }
    public class VE_PawnsInMap : VerbEffect {
		[FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        public override void RegisterAllTypes(VerbRootQD destination){
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Map map = Recast.recast<Map>(targetScope.quickEvaluate(context).singular());
            foreach(Pawn pawn in map.mapPawns.AllPawns){
                yield return pawn;
            }
        }
    }
    public class VE_AllHediffs : VerbEffect {
		[FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        public override void RegisterAllTypes(VerbRootQD destination){
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Pawn pawn = Recast.recast<Pawn>(targetScope.quickEvaluate(context).singular());
            foreach(Hediff hd in pawn.health.hediffSet.hediffs){
                yield return hd;
            }
        }
    }
    public class VE_AllFactions : VerbEffect {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            foreach(Faction faction in Find.FactionManager.AllFactions){
                yield return faction;
            }
        }
    }
}
