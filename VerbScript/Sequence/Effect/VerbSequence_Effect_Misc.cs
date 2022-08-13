using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using System.Xml;
using System.Reflection;
using System.Text.RegularExpressions;

namespace VerbScript {
    public class VE_FieldPropertyChainSetter : VerbEffect {
        //public Action<object, object>[] fieldSetter;
        public Func<object, object>[] fieldGetter;
        public Action<object, object> fieldSetter;
        //public Action<object, object>[] setter = (Action<MyClass, int>)Delegate.CreateDelegate(typeof(Action<MyClass, int>), null, typeof(MyClass).GetProperty("Number").GetSetMethod());  
        //public Func<object, object>[] getter = (Func<MyClass, int>)Delegate.CreateDelegate(typeof(Func<MyClass, int>), null, typeof(MyClass).GetProperty("Number").GetGetMethod());  

        public static char[] SA_splitChar = new char[]{ ',', '.', ':', ';' };
        //[DirectLoad]
        public static Action<VerbSequence, XmlNode, string> TCP_CUSTOMLOADER = (verbSeq, node, str) => {
            string valu = node.InnerText;
            VE_FieldPropertyChainSetter fc = ((VE_FieldPropertyChainSetter)verbSeq);
            string replaced = Regex.Replace(valu, @"\s|\t|\n", string.Empty);
            //Log.Warning("PRINT ORIGINAL " + valu);
            fc.fieldChain = replaced.Split(SA_splitChar);
            fc.fieldGetter = new Func<object, object>[fc.fieldChain.Length - 1];
            /**foreach(string strZ in fc.fieldChain){
                Log.Warning("PRINT PRINT " + strZ);
            }
            Log.Warning("EXECUTE FIN");**/
        };
        [FixedLoad(new string[]{ "fields", "field", "fieldpath" })][CustomLoad(typeof(VE_FieldPropertyChainSetter), "TCP_CUSTOMLOADER")]
        public string[] fieldChain;
        [FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        [FixedLoad][IndexedLoad(0)]
        public VerbSequence value;
        public override void RegisterAllTypes(VerbRootQD destination){
            base.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            value.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            value = value.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            targetScope.appendID();
            value.appendID();
            foreach(string str in fieldChain){
                SA_StringBuilder.Append(str);
            }
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            object obj = targetScope.quickEvaluate(context).singular();
            for(int i = 0; i < fieldChain.Length; i++){
                if(obj != null){
                    if(i == fieldChain.Length - 1){
                        if(fieldSetter == null){
                            FieldInfo info = obj.GetType().GetField(fieldChain[i]);
                            if(info == null){
                                PropertyInfo prop2 = obj.GetType().GetProperty(fieldChain[i]);
                                if(prop2 == null){
                                    Log.Error(fieldChain[i] + " is neither a field nor property of " + obj.GetType());
                                }
                                fieldSetter = (object objP, object objV) => {
                                    prop2.SetValue(objP, objV);
                                };
                            }else{
                                fieldSetter = (object objP, object objV) => {
                                    info.SetValue(objP, objV);
                                };
                            }
                        }
                        object valOut = value.quickEvaluate(context).singular();
                        //Log.Warning("Calc As " + valOut);
                        fieldSetter(obj, valOut);
                    }else{
                        if(fieldGetter[i] == null){
                            FieldInfo info = obj.GetType().GetField(fieldChain[i]);
                            if(info == null){
                                PropertyInfo prop2 = obj.GetType().GetProperty(fieldChain[i]);
                                if(prop2 == null){
                                    Log.Error(fieldChain[i] + " is neither a field nor property of " + obj.GetType());
                                }
                                fieldGetter[i] = (object objP) => {
                                    return prop2.GetValue(objP);
                                };
                            }else{
                                fieldGetter[i] = (object objP) => {
                                    return info.GetValue(objP);
                                };
                            }
                        }
                        obj = fieldGetter[i](obj);
                    }
                }else{
                    //yield return null;
                    //yield break;
                }
            }
            //yield return obj;
            yield break;
        }
    }
    public class VE_FieldPropertyChain : VerbEffect {
        //public Action<object, object>[] fieldSetter; //(Action<MyClass, int>)Delegate.CreateDelegate(typeof(Action<MyClass, int>), null, typeof(MyClass).GetProperty("Number").GetSetMethod());  
        public Func<object, object>[] fieldGetter; //(Func<MyClass, int>)Delegate.CreateDelegate(typeof(Func<MyClass, int>), null, typeof(MyClass).GetProperty("Number").GetGetMethod());  

        //public Action<object, object>[] setter = (Action<MyClass, int>)Delegate.CreateDelegate(typeof(Action<MyClass, int>), null, typeof(MyClass).GetProperty("Number").GetSetMethod());  
        //public Func<object, object>[] getter = (Func<MyClass, int>)Delegate.CreateDelegate(typeof(Func<MyClass, int>), null, typeof(MyClass).GetProperty("Number").GetGetMethod());  

        //[FixedLoad][IndexedLoad(0)]
        public static char[] SA_splitChar = new char[]{ ',', '.', ':', ';' };
        public VerbSequence targetScope = new VS_TopScope();
        [DirectLoad]
        public static Action<VerbSequence, XmlNode, string> CUSTOMLOADER = (verbSeq, node, str) => {
            VE_FieldPropertyChain fc = ((VE_FieldPropertyChain)verbSeq);
            string replaced = Regex.Replace(str, @"\s|\t|\n", string.Empty);
            fc.fieldChain = replaced.Split(SA_splitChar);
            //fc.fieldSetter = new Action<object, object>[fc.fieldChain.Length];
            fc.fieldGetter = new Func<object, object>[fc.fieldChain.Length];
        };
        public string[] fieldChain;
        public override void RegisterAllTypes(VerbRootQD destination){
            base.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            targetScope.appendID();
            foreach(string str in fieldChain){
                SA_StringBuilder.Append(str);
            }
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            object obj = targetScope.quickEvaluate(context).singular();
            for(int i = 0; i < fieldChain.Length; i++){
                if(obj != null){
                    if(fieldGetter[i] == null){
                        FieldInfo info = obj.GetType().GetField(fieldChain[i]);
                        if(info == null){
                            PropertyInfo prop2 = obj.GetType().GetProperty(fieldChain[i]);
                            if(prop2 == null){
                                Log.Error(fieldChain[i] + " is neither a field nor property of " + obj.GetType());
                            }
                            fieldGetter[i] = (object objP) => {
                                return prop2.GetValue(objP);
                            };
                        }else{
                            fieldGetter[i] = (object objP) => {
                                return info.GetValue(objP);
                            };
                        }
                    }
                    obj = fieldGetter[i](obj);
                }else{
                    yield return null;
                    yield break;
                }
            }
            yield return obj;
        }
    }
    public class VE_Delay : VerbEffect {
        [FixedLoad][RedirectLoad(typeof(VE_Number))][IndexedLoad(0)]//[DefaultType(typeof(VE_Number))]
        public VerbSequence delayTicks;
        public override void RegisterAllTypes(VerbRootQD destination){
            delayTicks.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            delayTicks = delayTicks.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            delayTicks.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            context.delayTicks += Recast.recast<int>(delayTicks.quickEvaluate(context).singular());
            return null;
        }
    }
    public class VE_TriggerIncident : VerbEffect {
        [FixedLoad][DefaultType(typeof(VE_IncidentDef))][IndexedLoad(0)]
        public VerbSequence incident;
        [FixedLoad]
        public VerbSequence targetMap = new VS_TopScope();
        [FixedLoad]
        public VerbSequence faction;
        [FixedLoad][DefaultType(typeof(VE_Number))]
        public VerbSequence points;

        //public VariableLoader incident;
        //public VariableLoader targetMap = new VariableLoader(){ sequence = new VS_TopScope() };

        //public VariableLoader faction;
        //public VariableLoader points;
        public override void RegisterAllTypes(VerbRootQD destination){
            incident.RegisterAllTypes(destination);
            targetMap.RegisterAllTypes(destination);
            faction?.RegisterAllTypes(destination);
            points?.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            incident = incident.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            targetMap = targetMap.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            faction = faction?.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            points = points?.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            incident.appendID();
            faction?.appendID();
            points?.appendID();
            targetMap.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            IncidentDef inciDef = Recast.recast<IncidentDef>(incident.quickEvaluate(context).singular());
            Map map = Recast.recast<Map>(targetMap.quickEvaluate(context).singular());
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(inciDef.category, map);
			if (inciDef.pointsScaleable){
				parms = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain).GenerateParms(inciDef.category, parms.target);
			}
            if(points != null){
                parms.points = Recast.recast<float>(points.quickEvaluate(context).singular());
            }
            if(faction != null){
                parms.faction = Recast.recast<Faction>(faction.quickEvaluate(context).singular());
            }
			inciDef.Worker.TryExecute(parms);
            yield break;
        }
    }
    public class VE_Print : VerbEffect {
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
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            object obj = targetScope.quickEvaluate(context).singular();
            Log.Warning("Print " + obj);
            yield break;
			//Log.Warning("Suspect C " + Rand.Value + " / " + new System.Diagnostics.StackTrace());
        }
    }

    public class VE_SetTerrain : VerbEffect {
        [FixedLoad][IndexedLoad(0)]
        public VerbSequence spawnPosition;
        [FixedLoad][DefaultType(typeof(VE_TerrainDef))][IndexedLoad(1)]
        public VerbSequence terrainDef;
        [FixedLoad]
        public VerbSequence targetScope = new VS_RootScope();
        public override void RegisterAllTypes(VerbRootQD destination){
            spawnPosition.RegisterAllTypes(destination);
            terrainDef.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            spawnPosition = spawnPosition.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            terrainDef = terrainDef.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            spawnPosition.appendID();
            terrainDef.appendID();
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            Map map = Recast.recast<Map>(targetScope.quickEvaluate(context).singular());
            IntVec3 vec3 = Recast.recast<IntVec3>(spawnPosition.quickEvaluate(context).singular());
            TerrainDef thingDef = Recast.recast<TerrainDef>(terrainDef.quickEvaluate(context).singular());
            if(vec3.InBounds(map)){
                map.terrainGrid.SetTerrain(vec3, thingDef);
            }
            yield break;
        }
    }

    public class VE_SpawnDef : VerbEffect {
        [FixedLoad][IndexedLoad(0)]
        public VerbSequence spawnPosition;
        [FixedLoad][DefaultType(typeof(VE_ThingDef))][IndexedLoad(1)]
        public VerbSequence spawnDef;
        [FixedLoad][DefaultType(typeof(VE_PawnKindDef))]
        public VerbSequence pawnkind;
        [FixedLoad][DefaultType(typeof(VE_ThingDef))]
        public VerbSequence stuffDef;
        [FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        [FixedLoad][RedirectLoad(typeof(VE_Number))]
        public VerbSequence scale = new VE_Number(){ number = 1 };
        public bool dontSpawnIfOccupied;
        public override void RegisterAllTypes(VerbRootQD destination){
            spawnPosition.RegisterAllTypes(destination);
            spawnDef.RegisterAllTypes(destination);
            pawnkind?.RegisterAllTypes(destination);
            stuffDef?.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            scale?.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            spawnPosition = spawnPosition.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            spawnDef = spawnDef.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            pawnkind = pawnkind?.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            stuffDef = stuffDef?.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            scale = scale?.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            SA_StringBuilder.Append(dontSpawnIfOccupied);
            SA_StringBuilder.Append("]");
            SA_StringBuilder.Append("[");
            spawnPosition.appendID();
            spawnDef.appendID();
            pawnkind?.appendID();
            stuffDef?.appendID();
            targetScope.appendID();
            scale?.appendID();
            SA_StringBuilder.Append("]");
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){//evaluate(Pawn pawn, ExecuteStackContext context, ExecuteStack exeStack){
            //Log.Warning("Suspect B " + Rand.Value + " / " + new System.Diagnostics.StackTrace());
            Map map = Recast.recast<Map>(targetScope.quickEvaluate(context).singular());
            IntVec3 vec3 = Recast.recast<IntVec3>(spawnPosition.quickEvaluate(context).singular());
            if(vec3.InBounds(map)){
                ThingDef thingDef = Recast.recast<ThingDef>(spawnDef.quickEvaluate(context).singular());
                if(thingDef.MadeFromStuff){
                    ThingDef stuffDe = (stuffDef == null)? null : Recast.recast<ThingDef>(stuffDef.quickEvaluate(context).singular());
                    Thing output = GenSpawn.Spawn(
                        ThingMaker.MakeThing(thingDef, stuffDe),
                        vec3,
                        map);
                    yield return output;
                }else if(thingDef.race != null){
                    PawnKindDef stuffDe = Recast.recast<PawnKindDef>(pawnkind.quickEvaluate(context).singular());
                    Thing output = PawnGenerator.GeneratePawn(stuffDe);
                    GenSpawn.Spawn(
                        output,
                        vec3,
                        map);
                    yield return output;
                }else if(thingDef.mote != null){
                    MoteMaker.MakeStaticMote(vec3, map, thingDef, Recast.recast<float>(scale.quickEvaluate(context).singular()));
                }else{
                    Thing output = GenSpawn.Spawn(
                        thingDef,
                        vec3,
                        map);
                    yield return output;
                }
            }
			//Log.Warning("Suspect C " + Rand.Value + " / " + new System.Diagnostics.StackTrace());
        }
    }
    public class VE_SummonLightning : VerbEffect {
        [FixedLoad]
        public VerbSequence targetScope = new VS_RootScope();
        [FixedLoad][IndexedLoad(0)]
        public VerbSequence spawnPosition;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            spawnPosition.appendID();
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override void RegisterAllTypes(VerbRootQD destination){
            spawnPosition.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            spawnPosition = spawnPosition.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            Map thing = Recast.recast<Map>(targetScope.quickEvaluate(context).singular());
            IntVec3 vec3 = Recast.recast<IntVec3>(spawnPosition.quickEvaluate(context).singular());
            thing.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(thing, vec3));
            //GenExplosion.DoExplosion(vec3, map, (int)Recast.recast<int>(radius.quickEvaluate(context).singular()), dDef, context.rootScope as Thing);   
            yield break;
        }
    }

	public class ExplosionStaticInstance : Explosion{
        public static ExplosionStaticInstance SA_unsafeExplosion;

    }
    public class VE_ThingDamage : VerbEffect {
        [FixedLoad][DefaultType(typeof(VE_DamageDef))][IndexedLoad(0)]
        public VerbSequence damageType = new VE_DamageDef(){ damageDef = DamageDefOf.EMP };
        [FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        [FixedLoad]
        public VerbSequence casterScope = new VS_RootScope();
        [FixedLoad][DefaultType(typeof(VE_Number))]
        public VerbSequence damageAmount = new VE_Number(){ number = -1 };
		[FixedLoad][DefaultType(typeof(VE_BodyPartDef))]
        public VerbSequence bodypart;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            damageAmount.appendID();
            damageType.appendID();
            targetScope.appendID();
            bodypart?.appendID();
            SA_StringBuilder.Append("]");
        }
        public override void RegisterAllTypes(VerbRootQD destination){
            damageType.RegisterAllTypes(destination);
            damageAmount.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            bodypart?.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            damageType = damageType.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            damageAmount = damageAmount.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            bodypart = bodypart?.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            Thing thing = Recast.recast<Thing>(targetScope.quickEvaluate(context).singular());
            Thing caster = Recast.recast<Thing>(casterScope.quickEvaluate(context).singular());
            DamageDef dDef = Recast.recast<DamageDef>(damageType.quickEvaluate(context).singular());
            int damageAmnt = (int)damageAmount.quickEvaluate(context).singular().recast<float>();
            BodyPartDef bodyPartDef = bodypart == null? null : Recast.recast<BodyPartDef>(bodypart.quickEvaluate(context).singular());
            BodyPartRecord partRecord = null;
            if(bodyPartDef != null){
                Pawn pawn = thing as Pawn;
                partRecord = pawn.RaceProps.body.AllParts.Find((BodyPartRecord x) => x.def == bodyPartDef);
            }
            DamageInfo dII = new DamageInfo(dDef, damageAmnt, 0, 0, null, partRecord, null, DamageInfo.SourceCategory.ThingOrUnknown, thing);
            thing.TakeDamage(dII);
            //GenExplosion.DoExplosion(vec3, map, (int)Recast.recast<int>(radius.quickEvaluate(context).singular()), dDef, context.rootScope as Thing);   
            yield break;
        }
    }
    public class VE_CellDamage : VerbEffect {
        [FixedLoad][DefaultType(typeof(VE_DamageDef))][IndexedLoad(0)]
        public VerbSequence damageType = new VE_DamageDef(){ damageDef = DamageDefOf.EMP };
        [FixedLoad][IndexedLoad(1)]
        public VerbSequence spawnPosition;
        [FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        [FixedLoad][DefaultType(typeof(VE_Number))]
        public VerbSequence radius = new VE_Number(){ number = 0 };
        [FixedLoad][DefaultType(typeof(VE_Number))]
        public VerbSequence damageAmount = new VE_Number(){ number = -1 };
        [FixedLoad][DefaultType(typeof(bool))]
        public bool doCameraShake = false;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + doCameraShake + "]");
            SA_StringBuilder.Append("[");
            spawnPosition.appendID();
            radius.appendID();
            damageAmount.appendID();
            damageType.appendID();
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override void RegisterAllTypes(VerbRootQD destination){
            spawnPosition.RegisterAllTypes(destination);
            radius.RegisterAllTypes(destination);
            damageType.RegisterAllTypes(destination);
            damageAmount.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            spawnPosition = spawnPosition.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            radius = radius.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            damageType = damageType.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            damageAmount = damageAmount.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            Map map = Recast.recast<Map>(targetScope.quickEvaluate(context).singular());
            IntVec3 vec3 = Recast.recast<IntVec3>(spawnPosition.quickEvaluate(context).singular());
            if(vec3.InBounds(map)){
                DamageDef dDef = Recast.recast<DamageDef>(damageType.quickEvaluate(context).singular());
                Harmony_VerbScriptHook.SA_DisableShake = !doCameraShake;
                try{
                    //dDef.Worker.ExplosionAffectCell(this, c, this.damagedThings, this.ignoredThings, !flag);
                    GenExplosion.DoExplosion(vec3, map, (int)Recast.recast<int>(radius.quickEvaluate(context).singular()), dDef, context.rootScope as Thing, (int)damageAmount.quickEvaluate(context).singular().recast<float>());
                        //ExplosionAffectCell(vec3, dDef, map);
                }catch(Exception e){
                    Log.Error(e.Message + " " + e.StackTrace);
                }finally{
                    Harmony_VerbScriptHook.SA_DisableShake = false;
                }
            }
            yield break;
        }
    }
    public class VE_CompTrigger : VerbEffect {
        [FixedLoad][IndexedLoad(0)]
        public VerbSequence activatorScope = new VS_TopScope();
        [FixedLoad][IndexedLoad(1)]
        public VerbSequence targetScope;
        public static Action<VerbSequence, XmlNode, string> TCP_CUSTOMLOADER = (verbSeq, node, str) => {
            VE_CompTrigger vct = (VE_CompTrigger)verbSeq;
            for(int i = 0; i < node.ChildNodes.Count; i++){
                //Log.Warning("OriginalText " + node.ChildNodes[i].OuterXml);
                //string strRegex = "AUTOGEN_BE1AAF0F_" + Regex.Replace(node.ChildNodes[i].OuterXml, @"\s+", string.Empty) + "_BE1AAF0F";
                //string strRegex = "AUTOGEN_BE1AAF0F_" + Regex.Replace(node.ChildNodes[i].OuterXml, @"\s|\n|\/|\\|\<|\>", String.Empty) + "_BE1AAF0F";
                int UIND = node.ChildNodes[i].OuterXml.registerUniqueReturnIndex();
                string strRegex = "AUTOGEN_BE1AAF0F_" + UIND + "_BE1AAF0F";
                vct.TCP_StringForm.Add(strRegex);
            }
            vct.comps = DirectXmlToObject.ObjectFromXml<List<CompProperties>>(node, false);
        
            for(int i = 0; i < vct.TCP_StringForm.Count; i++){
                if(ThingUseTrigger.stringToThingDefActivator.TryGetValue(vct.TCP_StringForm[i], out ThingDef thingDef)){
                    vct.thingDefCache.Add(thingDef);
                }else{
                    ThingDef thingDefNew = ThingUseTrigger.defaultGetter(vct.TCP_StringForm[i], vct.comps[i]);
                    ThingUseTrigger.stringToThingDefActivator.Add(vct.TCP_StringForm[i], thingDefNew);
                    vct.thingDefCache.Add(thingDefNew);
                }
            }
        };
        [FixedLoad][CustomLoad(typeof(VE_CompTrigger), "TCP_CUSTOMLOADER")]
        public List<CompProperties> comps = new List<CompProperties>();
        public List<string> TCP_StringForm = new List<string>();
        public List<ThingDef> thingDefCache = new List<ThingDef>();
        public override void appendID(){
            base.appendID();
            foreach(string str in TCP_StringForm){
                SA_StringBuilder.Append("[" + str + "]");
            }
            SA_StringBuilder.Append("[");
            activatorScope.appendID();
            targetScope?.appendID();
            SA_StringBuilder.Append("]");
        }
        public override void RegisterAllTypes(VerbRootQD destination){
            activatorScope.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            activatorScope = activatorScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            Pawn caster = Recast.recast<Thing>(activatorScope.quickEvaluate(context).singular()) as Pawn;
            Map map = caster.recast<Map>();
            Thing target = targetScope == null? null : Recast.recast<Thing>(targetScope.quickEvaluate(context).singular());
            //Log.Warning("LOG " + comps.Count);
            for(int i = 0; i < comps.Count; i++){
                ThingWithComps output = (ThingWithComps)GenSpawn.Spawn(
                    thingDefCache[i],
                    caster.Position,
                    map);
                foreach(CompTargetEffect cte in output.AllComps.Where(x => x is CompTargetEffect)){
                    //Log.Warning("Trigger! " + cte);
                    cte.DoEffectOn(caster, target);
                }
                output.Destroy();
            }
            yield break;
        }

    }
    /**public class VE_DestroyAt : VerbEffect {
        [FixedLoad][IndexedLoad(0)][RedirectLoad(typeof(VE_Vector3))]
        public VerbSequence destroyPosition;
        [FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            destroyPosition.appendID();
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override void RegisterAllTypes(VerbRootQD destination){
            destroyPosition.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            destroyPosition.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public static List<Thing> SA_AntiConcurrentModification = new List<Thing>();
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            Map map = Recast.recast<Map>(targetScope.quickEvaluate(context).singular());
            IntVec3 vec3 = Recast.recast<IntVec3>(destroyPosition.quickEvaluate(context).singular());
            if(vec3.InBounds(map)){
                SA_AntiConcurrentModification.Clear();
                SA_AntiConcurrentModification.AddRange(vec3.GetThingList(map));
                foreach(Thing thing in SA_AntiConcurrentModification){
                    thing.Destroy();
                }
            }
            yield break;
        }
    }**/
    public class VE_Destroy : VerbEffect {
        [FixedLoad][IndexedLoad(0)]
        public VerbSequence destroyEnumerable;
        [FixedLoad]
        public VerbSequence targetScope = new VS_TopScope();
        [FixedLoad][DefaultType(typeof(bool))]
        public bool destroyUnsafe = false;
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[" + destroyUnsafe + "]");
            SA_StringBuilder.Append("[");
            destroyEnumerable.appendID();
            targetScope.appendID();
            SA_StringBuilder.Append("]");
        }
        public override void RegisterAllTypes(VerbRootQD destination){
            destroyEnumerable.RegisterAllTypes(destination);
            targetScope.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            if(destroyEnumerable == null){
                destroyEnumerable = new VS_TopScope();
            }
            destroyEnumerable = destroyEnumerable.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            targetScope = targetScope.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public static List<Thing> SA_AntiConcurrentModificationThing = new List<Thing>();
        public static List<Hediff> SA_AntiConcurrentModificationHediff = new List<Hediff>();
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            Map map = Recast.recast<Map>(targetScope.quickEvaluate(context).singular());
            if(destroyUnsafe){
                SA_AntiConcurrentModificationThing.Clear();
                SA_AntiConcurrentModificationHediff.Clear();
                foreach(object thing in destroyEnumerable.quickEvaluate(context)){
                    if(thing.recastableTo(typeof(Hediff))){
                        SA_AntiConcurrentModificationHediff.Add(thing.recast<Hediff>());
                    }else{
                        SA_AntiConcurrentModificationThing.Add(thing.recast<Thing>());
                    }
                }
                foreach(Thing thing in SA_AntiConcurrentModificationThing){
                    thing.Destroy();
                }
                foreach(Hediff hediff in SA_AntiConcurrentModificationHediff){
                    hediff.pawn.health.RemoveHediff(hediff);
                    //hediff.Severity = 0;
                }
                SA_AntiConcurrentModificationThing.Clear();
                SA_AntiConcurrentModificationHediff.Clear();
            }else{
                SA_AntiConcurrentModificationThing.Clear();
                SA_AntiConcurrentModificationHediff.Clear();
                foreach(object thing in destroyEnumerable.quickEvaluate(context)){
                    //Log.Warning("Try destroy " + thing);
                    if(thing.recastableTo(typeof(Hediff))){
                        SA_AntiConcurrentModificationHediff.Add(thing.recast<Hediff>());
                    }else{
                        SA_AntiConcurrentModificationThing.Add(thing.recast<Thing>());
                    }
                }
                foreach(Thing thing in SA_AntiConcurrentModificationThing){
                    context.tryDestroy(thing);//.flagDestroy.Add(thing);
                }
                foreach(Hediff hediff in SA_AntiConcurrentModificationHediff){
                    context.tryDestroy(hediff);//context.flagDestroy.Add(hediff);
                }
                SA_AntiConcurrentModificationThing.Clear();
                SA_AntiConcurrentModificationHediff.Clear();
                /**foreach(object thing in destroyEnumerable.quickEvaluate(context)){
                    context.flagDestroy.Add(thing);
                }**/
            }
            yield break;
        }

    }

}
