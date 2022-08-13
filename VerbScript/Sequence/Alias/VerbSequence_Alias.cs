using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;


namespace VerbScript {
    public class VEC_LoadVariableLocal : VerbScope {
        [FixedLoad][DefaultType(typeof(VE_String))][IndexedLoad(0)][RedirectLoad(typeof(VE_String))]
        public VerbSequence variableName;
        [FixedLoad]
        public VerbSequence targetScope = new VS_RootScope();
        public override VerbSequence ResolveAlias(){
            VS_LoadVariable ll = new VS_LoadVariable();
            ll.holderType = VariableHolderType.Local;
            ll.variableName = variableName;
            ll.targetScope = targetScope;
            return ll;
        }
    }
    public class VEC_SaveVariableLocal : VerbEffect {//WARNING this type will not be saved
		[FixedLoad][DefaultType(typeof(VE_String))][IndexedLoad(0)]
        public VerbSequence variableName;
		[FixedLoad][IndexedLoad(1)]
        public VerbSequence variable;
		[FixedLoad]
        public VerbSequence targetScope = new VS_RootScope();
        public override VerbSequence ResolveAlias(){
            VE_SaveVariable ll = new VE_SaveVariable();
            ll.holderType = VariableHolderType.Local;
            ll.variableName = variableName;
            ll.variable = variable;
            ll.targetScope = targetScope;
            return ll;
        }
    }
    public class VEC_DestroyAt : VerbEffect {
        [FixedLoad][IndexedLoad(0)][RedirectLoad(typeof(VE_Vector3))]
        public VerbSequence destroyPosition;
        [FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        public override VerbSequence ResolveAlias(){
            VE_Destroy destroy = new VE_Destroy();
            destroy.destroyEnumerable = new VE_ThingsInRadius() { position = this.destroyPosition , targetScope = this.targetScope };
            return destroy;
        }
    }
    public class VEC_Repeat : VS_Groupable {
        [FixedLoad][IndexedLoad(0)][DefaultType(typeof(VE_Number))]
        public VerbSequence number;
        [ListLoad(typeof(VerbSequence))][XmlStringStackAttribute("verbSequences_textForm")]
        public List<VerbSequence> verbSequences = new List<VerbSequence>();

        public stringCH verbSequences_textForm;
        public override VerbSequence ResolveAlias(){
            int index = verbSequences_textForm.registerUniqueReturnIndex();
            string variableKey = "ProcGenVar_BE1AAF0F_" + index;
            //Log.Warning("Testing UniqueIndex " + index);
            VE_While whileLoop = new VE_While();
            VC_Lesser lesser = new VC_Lesser();
            whileLoop.condition = lesser;
            lesser.A = new VS_LoadVariable(){ variableName = new VE_String(){ text = variableKey } };
            lesser.B = number;
            whileLoop.verbSequences = verbSequences;
            whileLoop.verbSequences.Add(
                new VE_SaveVariable() {
                    variableName = new VE_String(){ text = variableKey },
                    variable = new VE_Add(){
                        param = new List<VerbSequence>(){
                            new VS_LoadVariable(){ variableName = new VE_String(){ text = variableKey } },
                            new VE_Number(){ number = 1.0f }
                        }
                    }
                }
            );
            return whileLoop;
        }
    }
    public class VEC_Ignore : VS_Groupable {
        public override VerbSequence ResolveAlias() {
            return new VE_Number(){ number = -1.0f };
        }
    }
    public class VEC_Chase : VS_Groupable {
        public override VerbSequence ResolveAlias() {
            return new VE_Number(){ number = 1.0f };
        }
    }
    public class VEC_Fire : VS_Groupable {
        public override VerbSequence ResolveAlias() {
            return new VE_Number(){ number = 0.0f };
        }
    }
}
