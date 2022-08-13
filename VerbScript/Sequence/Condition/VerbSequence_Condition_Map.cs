using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbScript {

    public class VC_Passable : VC_SingleScope{
        [FixedLoad][IndexedLoad(0)][RedirectLoad(typeof(VE_Vector3))]
        public VerbSequence position;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            position.appendID();
            SA_StringBuilder.Append("]");
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Map map = (targetScope.quickEvaluate(context).singular().recast<Map>());
            IntVec3 pos = (position.quickEvaluate(context).singular().recast<IntVec3>());
            yield return pos.Impassable(map)? -1 : 0;
        }
    }
    public class VC_Walkable : VC_SingleScope{
        [FixedLoad][IndexedLoad(0)][RedirectLoad(typeof(VE_Vector3))]
        public VerbSequence position;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            position.appendID();
            SA_StringBuilder.Append("]");
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Map map = (targetScope.quickEvaluate(context).singular().recast<Map>());
            IntVec3 pos = (position.quickEvaluate(context).singular().recast<IntVec3>());
            yield return pos.Walkable(map)? 0 : -1;
        }
    }
    public class VC_Standable: VC_SingleScope{
        [FixedLoad][IndexedLoad(0)][RedirectLoad(typeof(VE_Vector3))]
        public VerbSequence position;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            position.appendID();
            SA_StringBuilder.Append("]");
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<int> evaluateCT(ExecuteStackContext context){
            Map map = (targetScope.quickEvaluate(context).singular().recast<Map>());
            IntVec3 pos = (position.quickEvaluate(context).singular().recast<IntVec3>());
            yield return pos.Standable(map)? 0 : -1;
        }
    }
}
