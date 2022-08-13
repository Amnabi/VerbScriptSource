using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbScript {
    public class VE_Number : VerbEffect {
		[DirectLoad]
        public float number = 0.0f;
        public override void appendID() {
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(number.ToString());
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return number;
        }
    }
    public class VE_BodyPartDef : VerbEffect {
		[DirectLoad]
        public BodyPartDef bodyPartDef;
        public override void appendID() {
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(bodyPartDef.defName);
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return bodyPartDef;
        }
    }
    public class VE_ThingDef : VerbEffect {
		[DirectLoad]
        public ThingDef thingDef;
        public override void appendID() {
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(thingDef.ToString());
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return thingDef;
        }
    }
    public class VE_TerrainDef : VerbEffect {
		[DirectLoad]
        public TerrainDef terrainDef;
        public override void appendID() {
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(terrainDef.ToString());
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return terrainDef;
        }
    }
    public class VE_DamageDef : VerbEffect {
		[DirectLoad]
        public DamageDef damageDef;
        public override void appendID() {
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(damageDef.ToString());
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return damageDef;
        }
    }
    public class VE_IncidentDef : VerbEffect {
		[DirectLoad]
        public IncidentDef incidentDef;
        public override void appendID() {
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(incidentDef.ToString());
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return incidentDef;
        }
    }
    public class VE_HediffDef : VerbEffect {
		[DirectLoad]
        public HediffDef hediffDef;
        public override void appendID() {
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(hediffDef.ToString());
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return hediffDef;
        }
    }
    public class VE_PawnKindDef : VerbEffect {
		[DirectLoad]
        public PawnKindDef pawnKindDef;
        public override void appendID() {
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(pawnKindDef.ToString());
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return pawnKindDef;
        }
    }
    public class VE_MentalStateDef : VerbEffect {
		[DirectLoad]
        public MentalStateDef mentalStateDef;
        public override void appendID() {
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(mentalStateDef.ToString());
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return mentalStateDef;
        }
    }
    public class VE_TrainableDef : VerbEffect {
		[DirectLoad]
        public TrainableDef trainableDef;
        public override void appendID() {
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(trainableDef.ToString());
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return trainableDef;
        }
    }
    public class VE_String : VerbEffect {
		[DirectLoad]
        public string text;
        public override void appendID() {
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(text);
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return text;
        }
    }
}
