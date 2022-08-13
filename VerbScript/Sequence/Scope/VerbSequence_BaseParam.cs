using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;


namespace VerbScript {
    public class VS_LocalTarget : VerbScope {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            object obj = context.Param_TargetInfo;
            if(this.scopeRightType() == ScopeRightType.Return){
                yield return obj;
            }else{
                context.pushScope(context.Param_TargetInfo.Thing);
                if(this.scopeRightType() == ScopeRightType.ConditionParam){
                    for(int i = 0; i < this.verbSequences.Count; i++){
                        foreach(object retN in verbSequences[i].quickEvaluate(context)){
                            yield return retN;
                        }
                        //yield return verbSequences[i].quickEvaluate(context);
                    }
                }else{//nope
                }
            }
        }
    }

    public class VE_ExtraLocalTarget : VerbEffect {
        [FixedLoad][DefaultType(typeof(VE_Number))][IndexedLoad(0)][RedirectLoad(typeof(VE_Number))]
        public VerbSequence index;
        public override void RegisterAllTypes(VerbRootQD destination){
            index.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            index = index.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            index.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            int F = 1 + (int)Recast.recast<float>(index.quickEvaluate(context).singular());
            yield return context.Param_ExtraTargetInfo.elements[F].localTargetInfo;
        }
    }
}
