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
    public abstract class VerbEffect : VS_Groupable {
        //scope trick vs list
        public static List<List<object>> SA_FreeObjectLists = new List<List<object>>();
        public static List<object> nextObjectLists(){
            if(SA_FreeObjectLists.Count > 0){
                return SA_FreeObjectLists.Pop();
            }
            return new List<object>();
        }
        public static void freeList(List<object> un){
            un.Clear();
            SA_FreeObjectLists.Add(un);
        }
    }
    
    [StaticConstructorOnStartup]
    public class VE_ObjectPrimitive : VerbEffect {
        public static Dictionary<string, Type> stringToType = new Dictionary<string, Type>();
        public static void register(string str, Type type){
            stringToType.Add(str, type);
        }
        static VE_ObjectPrimitive(){
            register("bool", typeof(bool));
            register("boolean", typeof(bool));
            register("int", typeof(int));
            register("integer", typeof(int));
            register("float", typeof(float));
            register("number", typeof(float));
        }

        [DirectLoad]
        public static Action<VerbSequence, XmlNode, string> CUSTOMLOADER = (verbSeq, node, str) => {
            Vector3 v3 = (Vector3)ParseHelper.FromString(str, typeof(Vector3));
            VE_Vector3 vs3 = (VE_Vector3)verbSeq;
            vs3.x = new VE_Number(){ number = v3.x };
            vs3.y = new VE_Number(){ number = v3.y };
            vs3.z = new VE_Number(){ number = v3.z };
        };
        public object object_;
        public string originalString;

        public override void RegisterAllTypes(VerbRootQD destination){
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + originalString);
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return object_;
        }
    }
    
    public abstract class VE_NumberOperator : VerbEffect {
        [ListLoad(typeof(VerbSequence))]
        public List<VerbSequence> param = new List<VerbSequence>();
        public override void RegisterAllTypes(VerbRootQD destination){
            foreach(VerbSequence vSeq in param){
                vSeq.RegisterAllTypes(destination);
            }
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            for(int i = 0; i < param.Count; i++){
                param[i] = param[i].registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            }
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            foreach(VerbSequence vSeq in param){
                vSeq.appendID();
            }
            SA_StringBuilder.Append("]");
        }
    }
    public class VE_Modulo : VE_NumberOperator {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            yield return param[0].quickEvaluate(context).singular().recast<float>().modulo(param[1].quickEvaluate(context).singular().recast<float>());
            yield break;
        }
    }
    public class VE_Add : VE_NumberOperator {
        public static object DynamicOperation(object a, object b) {
            //Log.Warning("INPUT " + a + " / " + b);
            //Log.Warning("CALCOUT " + (Convert.ToDecimal(a) + Convert.ToDecimal(b)));
            return Convert.ChangeType(Convert.ToDecimal(a) + Convert.ToDecimal(b), a.GetType());
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            VerbSequence first = param[0];
            object firstV = first.quickEvaluate(context).singular();
            if(firstV.recastableTo(typeof(Vector3)) && firstV.recast<Vector3>() is Vector3 v3){
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    v3 += Recast.recast<Vector3>(vSeq.quickEvaluate(context).singular());
                }
                yield return v3;
            }else{
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    firstV = DynamicOperation(firstV, vSeq.quickEvaluate(context).singular());
                }
                yield return firstV;
            }
            /**else if(firstV.recastableTo(typeof(float)) && firstV.recast<float>() is float f){
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    f += Recast.recast<float>(vSeq.quickEvaluate(context).singular());
                }
                yield return f;
            }else{
                Log.Error("Unknown parameter type for Add " + firstV);
            }**/
        }
    }
    public class VE_Subtract : VE_NumberOperator {
        public static object DynamicOperation(object a, object b) {
            return Convert.ChangeType(Convert.ToDecimal(a) - Convert.ToDecimal(b), a.GetType());
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            VerbSequence first = param[0];
            object firstV = first.quickEvaluate(context).singular();
            if(firstV.recastableTo(typeof(Vector3)) && firstV.recast<Vector3>() is Vector3 v3){
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    v3 -= Recast.recast<Vector3>(vSeq.quickEvaluate(context).singular());
                }
                yield return v3;
            }else{
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    firstV = DynamicOperation(firstV, vSeq.quickEvaluate(context).singular());
                }
                yield return firstV;
            }
            /**
            else if(firstV.recastableTo(typeof(float)) && firstV.recast<float>() is float f){
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    f -= Recast.recast<float>(vSeq.quickEvaluate(context).singular());
                }
                yield return f;
            }else{
                Log.Error("Unknown parameter type for Subtract " + firstV);
            }**/
        }
    }
    public class VE_Multiply : VE_NumberOperator {
        public static object DynamicOperation(object a, object b) {
            return Convert.ChangeType(Convert.ToDecimal(a) * Convert.ToDecimal(b), a.GetType());
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            VerbSequence first = param[0];
            object firstV = first.quickEvaluate(context).singular();
            if(firstV.recastableTo(typeof(Vector3)) && firstV.recast<Vector3>() is Vector3 v3){
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    v3 *= Recast.recast<float>(vSeq.quickEvaluate(context).singular());
                }
                yield return v3;
            }else{
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    firstV = DynamicOperation(firstV, vSeq.quickEvaluate(context).singular());
                }
                yield return firstV;
            }
            /**
            else if(firstV.recastableTo(typeof(float)) && firstV.recast<float>() is float f){
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    f *= Recast.recast<float>(vSeq.quickEvaluate(context).singular());
                }
                yield return f;
            }else{
                Log.Error("Unknown parameter type for Multiply " + firstV);
            }**/
        }
    }
    public class VE_Divide : VE_NumberOperator {
        public static object DynamicOperation(object a, object b) {
            return Convert.ChangeType(Convert.ToDecimal(a) / Convert.ToDecimal(b), a.GetType());
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            VerbSequence first = param[0];
            object firstV = first.quickEvaluate(context).singular();
            if(firstV.recastableTo(typeof(Vector3)) && firstV.recast<Vector3>() is Vector3 v3){
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    v3 /= Recast.recast<float>(vSeq.quickEvaluate(context).singular());
                }
                yield return v3;
            }else{
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    firstV = DynamicOperation(firstV, vSeq.quickEvaluate(context).singular());
                }
                yield return firstV;
            }
            /**
            else if(firstV.recastableTo(typeof(float)) && firstV.recast<float>() is float f){
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    f /= Recast.recast<float>(vSeq.quickEvaluate(context).singular());
                }
                yield return f;
            }else{
                Log.Error("Unknown parameter type for Divide " + firstV);
            }**/
        }
    }
    public class VE_Max : VE_NumberOperator {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            VerbSequence first = param[0];
            object firstV = first.quickEvaluate(context).singular();
            if(firstV.recast<float>() is float f){
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    f = Mathf.Max(f, Recast.recast<float>(vSeq.quickEvaluate(context).singular()));
                }
                yield return f;
            }else{
                Log.Error("Unknown parameter type for Max " + firstV);
            }
        }
    }
    public class VE_Min : VE_NumberOperator {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            VerbSequence first = param[0];
            object firstV = first.quickEvaluate(context).singular();
            if(firstV.recast<float>() is float f){
                for(int i = 1; i < param.Count; i++){
                    VerbSequence vSeq = param[i];
                    f = Mathf.Min(f, Recast.recast<float>(vSeq.quickEvaluate(context).singular()));
                }
                yield return f;
            }else{
                Log.Error("Unknown parameter type for Min " + firstV);
            }
        }
    }
    public class VE_Random : VerbEffect {
        [FixedLoad][IndexedLoad(0)][DefaultType(typeof(VE_Number))]
        public VerbSequence minValue;
        [FixedLoad][IndexedLoad(1)][DefaultType(typeof(VE_Number))]
        public VerbSequence maxValue;
        public override void RegisterAllTypes(VerbRootQD destination){
            minValue.RegisterAllTypes(destination);
            maxValue.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            minValue = minValue.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            maxValue = maxValue.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            minValue.appendID();
            maxValue.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            float F = Recast.recast<float>(minValue.quickEvaluate(context).singular());
            float G = Recast.recast<float>(maxValue.quickEvaluate(context).singular());
            yield return Rand.Range(F, G);
        }
    }
    public class VE_Cosine : VerbEffect {
        [FixedLoad][DefaultType(typeof(VE_Number))][IndexedLoad(0)][RedirectLoad(typeof(VE_Number))]
        public VerbSequence angle;
        public override void RegisterAllTypes(VerbRootQD destination){
            angle.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            angle = angle.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            angle.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            float F = Recast.recast<float>(angle.quickEvaluate(context).singular());
            yield return Mathf.Cos(F / 180.0f * Mathf.PI);
        }
    }
    public class VE_Sine : VerbEffect {
        [FixedLoad][DefaultType(typeof(VE_Number))][IndexedLoad(0)][RedirectLoad(typeof(VE_Number))]
        public VerbSequence angle;
        public override void RegisterAllTypes(VerbRootQD destination){
            angle.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            angle = angle.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            angle.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            float F = Recast.recast<float>(angle.quickEvaluate(context).singular());
            yield return Mathf.Sin(F / 180.0f * Mathf.PI);
        }
    }
    public class VE_Tangent : VerbEffect {
        [FixedLoad][DefaultType(typeof(VE_Number))][IndexedLoad(0)][RedirectLoad(typeof(VE_Number))]
        public VerbSequence angle;
        public override void RegisterAllTypes(VerbRootQD destination){
            angle.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            angle = angle.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            angle.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            float F = Recast.recast<float>(angle.quickEvaluate(context).singular());
            yield return Mathf.Tan(F / 180.0f * Mathf.PI);
        }
    }
    public class VE_BracketBlock : VerbEffect {
        [ListLoad(typeof(VerbSequence))]
        public List<VerbSequence> verbSequences = new List<VerbSequence>();
        public override bool shouldRewindExecuteStack(ExecuteStackContext esc){
            return false;
        }
        public override bool shouldSkipSubExecuteStack(ExecuteStackContext esc){
            return false;
        }        
        public override int scriptSequenceGroup(ExecuteStackContext esc){
            return 0;
        }
        public override int scriptIndexMax(int scriptSequenceGroupSubIndex){
            return verbSequences.Count();
        }
        public override VerbSequence getSequenceAt(int i, int si){
            return verbSequences[i];
        }
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
            //condition.quickEvaluate(context).singular();
            //yield return output;
            yield break;
        }
    }
    public class VE_If : VerbEffect {
        public override bool applyNextOverride(XmlNode origin, VerbSequence nextElement){
            string lower = origin.Name.ToLower();
            switch(lower){
                case "else":{
                    if(else_ != null){
                        Log.Error("Else already exists!");
                    }
                    else_ = nextElement;
                    return true;
                    break;
                }
                case "else_if":
                case "else if":
                case "elseif":{
                    if(else_ != null){
                        Log.Error("Else already exists!");
                    }
                    else_ = nextElement;
                    return true;
                    break;
                }
            }
            return false;
        }

        [IndexedLoad(0)]
        public VerbSequence condition;
        [ListLoad(typeof(VerbSequence))]
        public List<VerbSequence> verbSequences = new List<VerbSequence>();
        
        public static Action<VerbSequence, XmlNode, string> TCP_CUSTOMLOADER = (verbSeq, node, str) => {
            VE_If this_ = (VE_If)verbSeq;
            string lower = node.Name.ToLower();
            switch(lower){
                case "else":{
                    this_.else_ = (VerbSequence)getGenericMethod(typeof(VE_BracketBlock)).Invoke(null, new object[] { node, null });
                    break;
                }
                case "else_if":
                case "else if":
                case "elseif":{
                    this_.else_ = (VerbSequence)getGenericMethod(typeof(VE_If)).Invoke(null, new object[] { node, null });
                    break;
                }
            }
        };
        //[FixedLoad(new string[]{ "else", "elseif", "else_if", "else if" })][CustomLoad(typeof(VE_If), "TCP_CUSTOMLOADER")]
        public VerbSequence else_; //special case
        public override bool shouldRewindExecuteStack(ExecuteStackContext esc){
            return false;
        }
        public override bool shouldSkipSubExecuteStack(ExecuteStackContext esc){
            if(condition.quickEvaluateAsBool(esc)){
                return false;
            }
            return else_ == null;//verbSequencesElse.Count == 0; //true;
        }        
        public override int scriptSequenceGroup(ExecuteStackContext esc){
            if(else_ == null){
                return 0;
            }
            if(condition.quickEvaluateAsBool(esc)){
                return 0;
            }
            return 1;
        }
        public override int scriptIndexMax(int scriptSequenceGroupSubIndex){
            return scriptSequenceGroupSubIndex == 0? verbSequences.Count() : 1;
        }
        public override VerbSequence getSequenceAt(int i, int si){
            return si == 0? verbSequences[i] : else_;
        }
        public override void RegisterAllTypes(VerbRootQD destination){
            else_?.RegisterAllTypes(destination);
            condition.RegisterAllTypes(destination);
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].RegisterAllTypes(destination);
            }
            base.RegisterAllTypes(destination);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            condition.appendID();
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].appendID();
            }
            else_?.appendID();
            SA_StringBuilder.Append("]");
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            condition = condition.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            else_ = else_?.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i] = verbSequences[i].registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            }
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            //condition.quickEvaluate(context).singular();
            //yield return output;
            yield break;
        }
    }
    public class VE_While : VerbEffect {
        [IndexedLoad(0)]
        public VerbSequence condition;
        [ListLoad(typeof(VerbSequence))]
        public List<VerbSequence> verbSequences = new List<VerbSequence>(); //acts like a pseudo - scope...!
        public override bool shouldRewindExecuteStack(ExecuteStackContext esc){
            return true;
        }
        public override bool shouldSkipSubExecuteStack(ExecuteStackContext esc){
            //VerbSequence.ClearCache();
            if(condition.quickEvaluateAsBool(esc)){
                return false;
            }
            return true;
        }
        public override int scriptIndexMax(int scriptSequenceGroupSubIndex){
            return verbSequences.Count();
        }
        public override VerbSequence getSequenceAt(int i, int subSequenceGroupIndex){
            return verbSequences[i];
        }
        public override void RegisterAllTypes(VerbRootQD destination){
            condition.RegisterAllTypes(destination);
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].RegisterAllTypes(destination);
            }
            base.RegisterAllTypes(destination);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            condition.appendID();
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i].appendID();
            }
            SA_StringBuilder.Append("]");
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            condition = condition.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i] = verbSequences[i].registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            }
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            //condition.quickEvaluate(context).singular();
            //yield return output;
            yield break;
        }
    }

}
