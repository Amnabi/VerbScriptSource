using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbScript {
    
    public abstract class VS_Groupable : VerbSequence{
        public override long CT_groupingHash() {
            return VerbSequenceNode.dynamicallyGeneratedIDMax + (typeID(new LongInt(PG_groupIndex(), scopeSequenceIndex)) / ID_Gap);
        }
    }
    public abstract class VerbCondition : VS_Groupable{
        public abstract IEnumerable<int> evaluateCT(ExecuteStackContext context);//{
            //throw new Exception("Not implemented");
            //yield break;
        //}
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            int USUB = uniqueSubIDFromContent();
            if(evaluateCT(context).Contains(USUB)){
                yield return true;
            }else{
                yield return false;
            }
        }
    }
    public abstract class VC_SingleScope : VerbCondition{
        [FixedLoad]//[IndexedLoad(0)]
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
    }
    public class VC_True: VerbCondition{
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            yield return 0;
        }
    }
    public class VC_False: VerbCondition{
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            yield return 1;
        }
    }
    public class VC_Equal : VerbCondition{
        [ListLoad(typeof(VerbSequence))]
        public List<VerbSequence> compareAll = new List<VerbSequence>();
        public override void RegisterAllTypes(VerbRootQD destination){
            for(int i = 0; i < compareAll.Count; i++){
                compareAll[i].RegisterAllTypes(destination);
            }
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            for(int i = 0; i < compareAll.Count; i++){
                compareAll[i] = compareAll[i].registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            }
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            for(int i = 0; i < compareAll.Count; i++){
                compareAll[i].appendID();
            }
            SA_StringBuilder.Append("]");
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            bool first = true;
            bool AND = true;
            object nuF = null;
            object neF = null;
            foreach(VerbSequence veq in compareAll){
                neF = veq.quickEvaluate(context).singular();
                if(first){
                    first = false;
                }else{
                    //if(neF != nuF){
                    if(!(neF == null && nuF == null) && !neF.Equals(nuF)){
                        //Log.Warning("FalseF " + (((object)10.0f).Equals((object)10.0f)));
                        yield return -1;
                        yield break;
                    }
                }
                nuF = neF;
            }
            //Log.Warning("True");
            yield return 0;
            //yield return (A.quickEvaluate(context).singular()) == (B.quickEvaluate(context).singular())? 0 : -1;
        }
    }
    public class VC_Lesser : VerbCondition{
        [IndexedLoad(0)]
        public VerbSequence A;
        [IndexedLoad(1)]
        public VerbSequence B;
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
            A.appendID();
            B.appendID();
            SA_StringBuilder.Append("]");
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            decimal decA = Convert.ToDecimal(A.quickEvaluate(context).singular());
            decimal decB = Convert.ToDecimal(B.quickEvaluate(context).singular());
            yield return decA < decB ? 0 : -1;
        }
    }
    public class VC_LesserEqual : VerbCondition{
        [IndexedLoad(0)]
        public VerbSequence A;
        [IndexedLoad(1)]
        public VerbSequence B;
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
            A.appendID();
            B.appendID();
            SA_StringBuilder.Append("]");
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            decimal decA = Convert.ToDecimal(A.quickEvaluate(context).singular());
            decimal decB = Convert.ToDecimal(B.quickEvaluate(context).singular());
            yield return decA <= decB ? 0 : -1;
        }
    }
    public class VC_Greater : VerbCondition{
        [IndexedLoad(0)]
        public VerbSequence A;
        [IndexedLoad(1)]
        public VerbSequence B;
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
            A.appendID();
            B.appendID();
            SA_StringBuilder.Append("]");
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            decimal decA = Convert.ToDecimal(A.quickEvaluate(context).singular());
            decimal decB = Convert.ToDecimal(B.quickEvaluate(context).singular());
            yield return decA > decB ? 0 : -1;
        }
    }
    public class VC_GreaterEqual : VerbCondition{
        [IndexedLoad(0)]
        public VerbSequence A;
        [IndexedLoad(1)]
        public VerbSequence B;
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
            A.appendID();
            B.appendID();
            SA_StringBuilder.Append("]");
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            decimal decA = Convert.ToDecimal(A.quickEvaluate(context).singular());
            decimal decB = Convert.ToDecimal(B.quickEvaluate(context).singular());
            yield return decA >= decB ? 0 : -1;
        }
    }
}
