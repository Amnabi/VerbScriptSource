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
    public class VC_IsType : VC_SingleScope{
		[FixedLoad][DefaultType(typeof(string))][DirectLoad]
        public string type;
        public Type cachedType;
        public static Dictionary<string, string> alias = new Dictionary<string, string>();
        static VC_IsType(){
            alias.Add("Pawn", "Verse.Pawn");
            alias.Add("pawn", "Verse.Pawn");
            alias.Add("Building", "Verse.Building");
            alias.Add("building", "Verse.Building");
            alias.Add("Thing", "Verse.Thing");
            alias.Add("thing", "Verse.Thing");

        }

        public Type getType{
            get{
                if(cachedType == null){
                    if(alias.ContainsKey(type)){
                        type = alias[type];
                    }
                    cachedType = MiscUtility.typeFromString(type);
                }
                return cachedType;
            }
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override void appendID(){
            base.appendID();
            if(alias.ContainsKey(type)){
                type = alias[type];
            }
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(type);
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            object obj = targetScope.quickEvaluate(context).singular().recast(getType);
            yield return (obj != null && getType.IsAssignableFrom(obj.GetType()))? 0 : -1;
        }
    }
    public class VC_IsNull : VerbCondition{
        [FixedLoad][IndexedLoad(0)]
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
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            object obj = targetScope.quickEvaluate(context).singular();
            yield return (obj == null)? 0 : -1;
        }
    }
    public class VC_NotNull : VC_SingleScope{
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override void appendID(){
            base.appendID();
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            object obj = targetScope.quickEvaluate(context).singular();
            yield return (obj != null)? 0 : -1;
        }
    }
}
