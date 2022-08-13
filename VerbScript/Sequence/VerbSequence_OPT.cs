using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbScript {
    public class VE_OPT_A8717DF : VerbEffect {
        public VerbSequence A;
        public VerbSequence B;
        public float C;
        public override void RegisterAllTypes(VerbRootQD destination){
            A.RegisterAllTypes(destination);
            B.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            A = A.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            B = B.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(C.ToString());
            SA_StringBuilder.Append("]");
            SA_StringBuilder.Append("[");
            A.appendID();
            B.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            float VR = Recast.recast<float>(A.quickEvaluate(context).singular());
            if(VR == -1.0f){
                yield return -1.0f;
                yield break;
            }
            if(VR == 1.0f){
                Log.Warning(" " + Recast.recast<float>(B.quickEvaluate(context).singular()));
							
                yield return Recast.recast<float>(B.quickEvaluate(context).singular());
                yield break;
            }
            LocalTargetInfo obj = context.scope(context.scopeStack.Count - 1).recast<LocalTargetInfo>();
            Thing root = context.rootScope as Thing;
            Map map = root.recast<Map>();
            if(obj.Cell.DistanceTo(root.Position) < C && GenSight.LineOfSight(root.Position, obj.Cell, map)){
                Log.Warning(" " + Recast.recast<float>(B.quickEvaluate(context).singular()));
                yield return Recast.recast<float>(B.quickEvaluate(context).singular());
                yield break;
            }
            yield return -1.0f;
            yield break;
        }
    }

    public struct TargetAndMode{
        public LocalTargetInfo targetInfo;
        public float targetMode;
        public override string ToString() {
            return targetInfo + " / " + targetMode;
        }

    }
    
    public class VE_OPT_A8717E0 : VerbEffect {
        public VerbSequence ENUMERABLE;
        //public VerbSequence EXTRAFILTER;
        public VerbSequence POINTEVALUATE;
        public VerbSequence MODEEVALUATE;
        public float RANGE;
        public override void RegisterAllTypes(VerbRootQD destination){
            ENUMERABLE.RegisterAllTypes(destination);
            POINTEVALUATE.RegisterAllTypes(destination);
            MODEEVALUATE.RegisterAllTypes(destination);
            //EXTRAFILTER?.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            ENUMERABLE = ENUMERABLE.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            POINTEVALUATE = POINTEVALUATE.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            MODEEVALUATE = MODEEVALUATE.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(RANGE);
            ENUMERABLE.appendID();
            POINTEVALUATE.appendID();
            MODEEVALUATE.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            float bestMatch = 0;
            float bestMode = -2;
            object target = null;
            IntVec3 rootIV3 = context.rootScope.recast<IntVec3>();
            Map map = context.rootScope.recast<Map>();
            foreach(object obj in ENUMERABLE.quickEvaluate(context)){
                context.pushScopeNoPointer(obj);
                float mode = MODEEVALUATE.quickEvaluate(context).singular().recast<float>();
                if(mode == -1.0f){
                    continue;
                }
                float value = -1.0f;
                IntVec3 asIV3 = obj.recast<IntVec3>();
                if(mode == 1.0f || (mode == 0.0f && asIV3.DistanceTo(rootIV3) <= RANGE && GenSight.LineOfSight(rootIV3, asIV3, map))){
                    value = POINTEVALUATE.quickEvaluate(context).singular().recast<float>();
                }
                if(value > bestMatch){
                    bestMatch = value;
                    bestMode = mode;
                    target = obj;
                }
                context.popScopeNoPointer();
            }
            if(target == null){
                yield return new TargetAndMode() {
                    targetMode = -1.0f,
                    targetInfo = null
                };
                yield break;
            }
            yield return new TargetAndMode() {
                targetMode = bestMode,
                targetInfo = target.recast<LocalTargetInfo>()
            };
        }
    }
}
