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
    public class VE_Vector3 : VerbEffect {
        [DirectLoad]
        public static Action<VerbSequence, XmlNode, string> CUSTOMLOADER = (verbSeq, node, str) => {
            Vector3 v3 = (Vector3)ParseHelper.FromString(str, typeof(Vector3));
            VE_Vector3 vs3 = (VE_Vector3)verbSeq;
            vs3.x = new VE_Number(){ number = v3.x };
            vs3.y = new VE_Number(){ number = v3.y };
            vs3.z = new VE_Number(){ number = v3.z };
        };
		[FixedLoad][DefaultType(typeof(VE_Number))]
        public VerbSequence x;
		[FixedLoad][DefaultType(typeof(VE_Number))]
        public VerbSequence y;
		[FixedLoad][DefaultType(typeof(VE_Number))]
        public VerbSequence z;
        public override void RegisterAllTypes(VerbRootQD destination){
            x.RegisterAllTypes(destination);
            y.RegisterAllTypes(destination);
            z.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            x = x.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            y = y.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            z = z.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            x.appendID();
            y.appendID();
            z.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            yield return new Vector3(
                Recast.recast<float>(x.quickEvaluate(context).singular()),
                Recast.recast<float>(y.quickEvaluate(context).singular()),
                Recast.recast<float>(z.quickEvaluate(context).singular())
            );
        }
    }
    
    //Vector rotate
    public class VE_Rotate : VerbEffect {
        [FixedLoad][DefaultType(typeof(VE_Number))][IndexedLoad(0)]
        public VerbSequence angle;
        [FixedLoad][IndexedLoad(1)]
        public VerbSequence vector;
        public override void RegisterAllTypes(VerbRootQD destination){
            angle.RegisterAllTypes(destination);
            vector.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            angle = angle.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            vector = vector.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            angle.appendID();
            vector.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Vector3 vec3 = Recast.recast<Vector3>(vector.quickEvaluate(context).singular());
            float ang = Recast.recast<float>(angle.quickEvaluate(context).singular());
            yield return vec3.RotatedBy(ang);
        }
    }
    public class VE_Normalize : VerbEffect {
        [IndexedLoad(0)]//[FixedLoad]
        public VerbSequence vector;
        [FixedLoad][DefaultType(typeof(bool))]
        public bool YZero = false;
        public override void RegisterAllTypes(VerbRootQD destination){
            vector.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            vector = vector.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(YZero);
            vector.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Vector3 vec3 = Recast.recast<Vector3>(vector.quickEvaluate(context).singular());
            if(YZero){
                vec3.y = 0;
            }
            yield return vec3.normalized;
        }
    }
    public class VE_DistanceYNormalized : VerbEffect {
        [IndexedLoad(0)]//[FixedLoad]
        public VerbSequence vectorA;
        [IndexedLoad(1)]//[FixedLoad]
        public VerbSequence vectorB;
        public override void RegisterAllTypes(VerbRootQD destination){
            vectorA.RegisterAllTypes(destination);
            vectorB.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            vectorA = vectorA.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            vectorB = vectorB.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            vectorA.appendID();
            vectorB.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Vector3 vec3A = Recast.recast<Vector3>(vectorA.quickEvaluate(context).singular());
            Vector3 vec3B = Recast.recast<Vector3>(vectorB.quickEvaluate(context).singular());
            vec3A.y = 0;
            vec3B.y = 0;
            yield return (vec3A - vec3B).magnitude;//vec3.normalized;
        }
    }
}
