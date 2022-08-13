using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbScript {

    public enum ScopeRightType{
        Return,
        Effect,
        ConditionParam
    }
    public enum ScopeLeftType{
        Root,
        CPE
    }

    public static class SLTHelper{
        public static ScopeLeftType mask(this ScopeLeftType left, ScopeLeftType mask){
            if(mask == ScopeLeftType.CPE){
                return mask;
            }
            return left;
        }
    }

    //Scope
    public class VerbScope : VerbSequence{
        [ListLoad(typeof(VerbSequence))]
        public List<VerbSequence> verbSequences = new List<VerbSequence>();
        public ScopeRightType rightHandScopeType;
        public ScopeLeftType leftHandScopeType;
        public override int scriptIndexMax(int scriptSequenceGroupSubIndex){
            return verbSequences.Count();
        }
        public override VerbSequence getSequenceAt(int i, int subSequenceGroupIndex){
            return verbSequences[i];
        }
        public override void RegisterAllTypes(VerbRootQD destination){
            foreach(VerbSequence ve in verbSequences){
                ve.RegisterAllTypes(destination);
            }
            base.RegisterAllTypes(destination);
        }
        private AON<VerbSequence> aonPrivate;
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            leftHandScopeType = leftHand;
            List<VerbScope> switcheroo = nextVerbScopeList();
            switcheroo.AddRange(verbScopesParent);
            switcheroo.Add(this);
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i] = verbSequences[i].registerAllSubVerbSequencesAndReturn(switcheroo, leftHand);
            }
            free(switcheroo);
            if(verbSequences.Count == 0){
                rightHandScopeType = ScopeRightType.Return;
            }else{
                foreach(VerbSequence verb in this.skipScopeRightTypeEnumerable()){
                    rightHandScopeType = verb is VerbEffect? ScopeRightType.Effect : ScopeRightType.ConditionParam;
                    break;
                }
            }
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(leftHandScopeType.ToString());
            SA_StringBuilder.Append("]");
            if(verbSequences.Count > 0){
                SA_StringBuilder.Append("[");
                for(int i = 0; i < verbSequences.Count; i++){
                    verbSequences[i].appendID();
                }
                SA_StringBuilder.Append("]");
            }
        }
        public ScopeRightType scopeRightType(){
            return rightHandScopeType;//verbEffects.Count == 0 ? ScopeRightType.ConditionParam : ScopeRightType.Effect;
        }
        public ScopeLeftType scopeLeftType(){
            return leftHandScopeType;//verbEffects.Count == 0 ? ScopeRightType.ConditionParam : ScopeRightType.Effect;
        }

        public virtual IEnumerable<VerbSequence> skipScopeRightTypeEnumerable(){
            if(this.scopeRightType() == ScopeRightType.Return){
                yield return this;
                yield break;
            }else{
                foreach(VerbSequence vs in verbSequences){
                    if(vs is VerbScope vsS){
                        foreach(VerbSequence vs2 in vsS.skipScopeRightTypeEnumerable()){
                            yield return vs2;
                        }
                    }else{
                        yield return vs;
                    }
                }
            }
        }

    }
    public class VS_TopScope : VerbScope {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            object obj = context.scope(context.scopeStack.Count - 1);
            if(this.scopeRightType() == ScopeRightType.Return){
                yield return obj;
            }else{
                context.pushScope(obj);
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
    public class VS_PrevScope : VerbScope {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            object obj = context.scope(context.scopeStack.Count - 2);
            if(this.scopeRightType() == ScopeRightType.Return){
                yield return obj;
            }else{
                context.pushScope(obj);
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
    public class VS_PrevPrevScope : VerbScope {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            object obj = context.scope(context.scopeStack.Count - 3);
            if(this.scopeRightType() == ScopeRightType.Return){
                yield return obj;
            }else{
                context.pushScope(obj);
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
    public class VS_RootScope : VerbScope {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            object obj = context.scope(-1);
            if(this.scopeRightType() == ScopeRightType.Return){
                yield return obj;
            }else{
                context.pushScope(obj);
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
}
