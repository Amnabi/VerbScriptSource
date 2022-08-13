using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbScript {
    //
    public class VS_LoadVariable : VerbScope {
		[FixedLoad][DefaultType(typeof(VariableHolderType))]
        public VariableHolderType holderType = VariableHolderType.TempScript; 
        [FixedLoad][DefaultType(typeof(VE_String))][IndexedLoad(0)][RedirectLoad(typeof(VE_String))]
        public VerbSequence variableName;
        [FixedLoad]
        public VerbSequence targetScope = new VS_RootScope();
        public override void RegisterAllTypes(VerbRootQD destination){
            variableName.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            variableName = variableName.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + holderType + "]");
            SA_StringBuilder.Append("[");
            variableName.appendID();
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Thing scope = Recast.recast<Thing>(targetScope.quickEvaluate(context).singular());
            VariableHolder SLAt = null;
            if(scope == context.scopeNow){
                if(holderType == VariableHolderType.TempScript){
                    SLAt = context.localVariableHolder;
                }else{
                    SLAt = context.thingVariableHolder;
                }
            }else{
                Comp_VerbHolder cvh;
                if((cvh = scope.TryGetComp<Comp_VerbHolder>()) != null){
                    SLAt = cvh.variableHolder;
                }else{
                    Comp_ScriptExecutor cse;
                    if((cse = scope.TryGetComp<Comp_ScriptExecutor>()) != null){
                        SLAt = cse.variableHolder;
                    }
                }
            }
            string key = Recast.recast<string>(variableName.quickEvaluate(context).singular());

            object obj = null;
            if(SLAt.variables.TryGetValue(key, out obj)){
            }

            //object obj = SLAt.variables[key];
            

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
                }else{
                }
            }
        }
    }
    public class VE_SaveVariable : VerbEffect {//WARNING this type will not be saved
		[FixedLoad][DefaultType(typeof(VariableHolderType))]
        public VariableHolderType holderType = VariableHolderType.TempScript; 
		[FixedLoad][DefaultType(typeof(VE_String))][IndexedLoad(0)]
        public VerbSequence variableName;
		[FixedLoad][IndexedLoad(1)]
        public VerbSequence variable;
		[FixedLoad]
        public VerbSequence targetScope = new VS_RootScope();
        public override void RegisterAllTypes(VerbRootQD destination){
            variableName.RegisterAllTypes(destination);
            variable.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            variableName = variableName.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            variable = variable.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + holderType + "]");
            SA_StringBuilder.Append("[");
            variableName.appendID();
            variable.appendID();
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Thing scope = Recast.recast<Thing>(targetScope.quickEvaluate(context).singular());
            VariableHolder SLAt = null;
            if(scope == context.rootScope){
                if(holderType == VariableHolderType.TempScript){
                    SLAt = context.localVariableHolder;
                }else{
                    SLAt = context.thingVariableHolder;
                }
            }else{
                Comp_VerbHolder cvh;
                if((cvh = scope.TryGetComp<Comp_VerbHolder>()) != null){
                    SLAt = cvh.variableHolder;
                }else{
                    Comp_ScriptExecutor cse;
                    if((cse = scope.TryGetComp<Comp_ScriptExecutor>()) != null){
                        SLAt = cse.variableHolder;
                    }
                }
            }
            string str = Recast.recast<string>(variableName.quickEvaluate(context).singular());
            object obj = variable.quickEvaluate(context).singular();
            VariableType vTy = VariableType.Unknown;
            if(obj is Thing){
                vTy = VariableType.Thing;
            }
            else if(obj is Hediff){
                vTy = VariableType.Hediff;
            }
            else if(obj is string){
                vTy = VariableType.String;
            }
            else if(obj is Vector3){
                vTy = VariableType.Vector3;
            }
            else if(obj is float){
                vTy = VariableType.Float;
            }
            SLAt.saveVariable(str, obj, vTy);
            return null;
        }
    }
    public class VE_SaveVariableThing : VE_SaveVariable {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Thing scope = Recast.recast<Thing>(targetScope.quickEvaluate(context).singular());
            VariableHolder SLAt = null;
            if(scope == context.rootScope){
                if(holderType == VariableHolderType.TempScript){
                    SLAt = context.localVariableHolder;
                }else{
                    SLAt = context.thingVariableHolder;
                }
            }else{
                Comp_VerbHolder cvh;
                if((cvh = scope.TryGetComp<Comp_VerbHolder>()) != null){
                    SLAt = cvh.variableHolder;
                }else{
                    Comp_ScriptExecutor cse;
                    if((cse = scope.TryGetComp<Comp_ScriptExecutor>()) != null){
                        SLAt = cse.variableHolder;
                    }
                }
            }string str = Recast.recast<string>(variableName.quickEvaluate(context).singular());
            SLAt.saveVariable(str,  Recast.recast<Thing>(variable.quickEvaluate(context).singular()), VariableType.Thing);
            return null;
        }
    }
    public class VE_SaveVariableHediff : VE_SaveVariable {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Thing scope = Recast.recast<Thing>(targetScope.quickEvaluate(context).singular());
            VariableHolder SLAt = null;
            if(scope == context.scopeNow){
                if(holderType == VariableHolderType.TempScript){
                    SLAt = context.localVariableHolder;
                }else{
                    SLAt = context.thingVariableHolder;
                }
            }else{
                Comp_VerbHolder cvh;
                if((cvh = scope.TryGetComp<Comp_VerbHolder>()) != null){
                    SLAt = cvh.variableHolder;
                }else{
                    Comp_ScriptExecutor cse;
                    if((cse = scope.TryGetComp<Comp_ScriptExecutor>()) != null){
                        SLAt = cse.variableHolder;
                    }
                }
            }string str = Recast.recast<string>(variableName.quickEvaluate(context).singular());
            SLAt.saveVariable(str,  Recast.recast<Hediff>(variable.quickEvaluate(context).singular()), VariableType.Hediff);
            return null;
        }
    }
    public class VE_SaveVariableFloat : VE_SaveVariable {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Thing scope = Recast.recast<Thing>(targetScope.quickEvaluate(context).singular());
            VariableHolder SLAt = null;
            if(scope == context.scopeNow){
                if(holderType == VariableHolderType.TempScript){
                    SLAt = context.localVariableHolder;
                }else{
                    SLAt = context.thingVariableHolder;
                }
            }else{
                Comp_VerbHolder cvh;
                if((cvh = scope.TryGetComp<Comp_VerbHolder>()) != null){
                    SLAt = cvh.variableHolder;
                }else{
                    Comp_ScriptExecutor cse;
                    if((cse = scope.TryGetComp<Comp_ScriptExecutor>()) != null){
                        SLAt = cse.variableHolder;
                    }
                }
            }string str = Recast.recast<string>(variableName.quickEvaluate(context).singular());
            SLAt.saveVariable(str,  Recast.recast<float>(variable.quickEvaluate(context).singular()), VariableType.Float);
            return null;
        }
    }
    public class VE_SaveVariableVector3 : VE_SaveVariable {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Thing scope = Recast.recast<Thing>(targetScope.quickEvaluate(context).singular());
            VariableHolder SLAt = null;
            if(scope == context.scopeNow){
                if(holderType == VariableHolderType.TempScript){
                    SLAt = context.localVariableHolder;
                }else{
                    SLAt = context.thingVariableHolder;
                }
            }else{
                Comp_VerbHolder cvh;
                if((cvh = scope.TryGetComp<Comp_VerbHolder>()) != null){
                    SLAt = cvh.variableHolder;
                }else{
                    Comp_ScriptExecutor cse;
                    if((cse = scope.TryGetComp<Comp_ScriptExecutor>()) != null){
                        SLAt = cse.variableHolder;
                    }
                }
            }string str = Recast.recast<string>(variableName.quickEvaluate(context).singular());
            SLAt.saveVariable(str,  Recast.recast<Vector3>(variable.quickEvaluate(context).singular()), VariableType.Vector3);
            return null;
        }
    }
    public class VE_SaveVariableString : VE_SaveVariable {
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Thing scope = Recast.recast<Thing>(targetScope.quickEvaluate(context).singular());
            VariableHolder SLAt = null;
            if(scope == context.scopeNow){
                if(holderType == VariableHolderType.TempScript){
                    SLAt = context.localVariableHolder;
                }else{
                    SLAt = context.thingVariableHolder;
                }
            }else{
                Comp_VerbHolder cvh;
                if((cvh = scope.TryGetComp<Comp_VerbHolder>()) != null){
                    SLAt = cvh.variableHolder;
                }else{
                    Comp_ScriptExecutor cse;
                    if((cse = scope.TryGetComp<Comp_ScriptExecutor>()) != null){
                        SLAt = cse.variableHolder;
                    }
                }
            }string str = Recast.recast<string>(variableName.quickEvaluate(context).singular());
            SLAt.saveVariable(str,  Recast.recast<string>(variable.quickEvaluate(context).singular()), VariableType.String);
            return null;
        }
    }
}
