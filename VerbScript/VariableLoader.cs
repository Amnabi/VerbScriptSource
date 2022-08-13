using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using System.Xml;

namespace VerbScript {

    public class TypeLoader{
        public static Dictionary<Type, TypeLoader> typeToTypeLoader = new Dictionary<Type, TypeLoader>();
        public static TypeLoader GetTypeLoader(Type type){
            if(typeToTypeLoader.TryGetValue(type, out TypeLoader typeLoader)){
                return typeLoader;
            }
            TypeLoader typeLoad = new TypeLoader();
            //Log.Warning(GetType().ToString());
            foreach(object obj in type.GetCustomAttributes(true)){
                Log.Warning("" + obj);
                FieldLoader createdIfAny = null;
            }

            foreach(FieldInfo fieldInf in type.GetFields()){
                FieldLoader createdIfAny = null;
                foreach(object obj in fieldInf.GetCustomAttributes(true)){
                    if(obj is ListLoadAttribute lla){
                        if(createdIfAny==null){ createdIfAny = new FieldLoader(); }
                        typeLoad.listDestination = createdIfAny;
                        createdIfAny.genericType = lla.type;
                    }else if(obj is IndexedLoadAttribute ila){
                        if(createdIfAny==null){ createdIfAny = new FieldLoader(); }
                        createdIfAny.order = ila.loadOrder;
                        typeLoad.listedLoader.Add(createdIfAny);
                    }else if(obj is DirectLoadAttribute dla){
                        if(createdIfAny==null){ createdIfAny = new FieldLoader(); }
                        typeLoad.directLoader = createdIfAny;
                        if(fieldInf.IsStatic && fieldInf.FieldType == typeof(Action<VerbSequence, XmlNode, string>)){
                            typeLoad.directLoader.customDirect = (Action<VerbSequence, XmlNode, string>)fieldInf.GetValue(null);
                        }
                        createdIfAny.defaultLoadType = fieldInf.FieldType;
                    }else if(obj is CustomLoadAttribute cla){
                        if(createdIfAny==null){ createdIfAny = new FieldLoader(); }
                        createdIfAny.customDirect = cla.actionSequence;
                    }else if(obj is RedirectLoadAttribute rla){
                        if(createdIfAny==null){ createdIfAny = new FieldLoader(); }
                        typeLoad.redirectLoader = createdIfAny;
                        createdIfAny.defaultLoadType = rla.type;
                    }else if(obj is DefaultTypeAttribute dta){
                        if(createdIfAny==null){ createdIfAny = new FieldLoader(); }
                        createdIfAny.defaultLoadType = dta.type;
                    }else if(obj is FixedLoadAttribute fla){
                        if(createdIfAny==null){ createdIfAny = new FieldLoader(); }
                        typeLoad.fixedStringToFieldLoader.Add(fieldInf.Name.ToLower(), createdIfAny);
                        if(fla.alias != null){
                            foreach(string str in fla.alias){//lowercase just in case
                                typeLoad.fixedStringToFieldLoader.Add(str.ToLower(), createdIfAny);
                            }
                        }
                    }
                }
                if(createdIfAny != null){
                    createdIfAny.fieldName = fieldInf.Name;
                    if(createdIfAny.defaultLoadType != null && typeLoad.directLoader != createdIfAny && !typeLoad.fixedStringToFieldLoader.ContainsValue(createdIfAny)){
                        Log.Warning("Has default load type but no fixed String! " + createdIfAny.fieldName + " of Type " + type);
                    }
                }
                //Log.Warning(fieldInf.Name);
            }
            typeLoad.listedLoader.SortBy(x => x.order);
            typeToTypeLoader.Add(type, typeLoad);
            return typeLoad;
        }
				//this.offset = (Vector2)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(Vector2));
        public TypeLoader(){
        }
        public FieldLoader listDestination;
        public FieldLoader redirectLoader;
        public FieldLoader directLoader;
        public List<FieldLoader> listedLoader = new List<FieldLoader>();
        public Dictionary<string, FieldLoader> fixedStringToFieldLoader = new Dictionary<string, FieldLoader>();
    }
    public class FieldLoader{
        public Type defaultLoadType;
        public Type genericType;
        public string fixedString;
        public string fieldName;
        public int order;
        public int loadType_Cache = -1; //0 value 1 ref 2 seq
        public FieldInfo cached_FieldInfo;
        public MethodInfo cached_MethodInfo;
        public Action<VerbSequence, XmlNode, string> customDirect;
        public int loadType{
            get{
                if(loadType_Cache == -1){
                    if(typeof(VerbSequence).IsAssignableFrom(defaultLoadType)){
                        loadType_Cache = 2;
                    }else if(typeof(Def).IsAssignableFrom(defaultLoadType)){
                        loadType_Cache = 1;
                    }else{
                        loadType_Cache = 0;
                    }
                    
                }
                return loadType_Cache;
            }
        }
        
        public void apply(object instance, object obj){
            if(cached_FieldInfo == null){
                cached_FieldInfo = instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            }
            cached_FieldInfo.SetValue(instance, obj);
        }
        public void applyElement(object instance, object obj){
            if(cached_MethodInfo == null){
                cached_FieldInfo = instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                //cached_MethodInfo = typeof(List<>).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Last(x => x.Name == "Add"); //typeof(List<>).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(object) }, null);
                
                cached_MethodInfo = typeof(IList).GetMethod("Add", new Type[] { typeof(object) });
                //cached_MethodInfo = cached_MethodInfo.MakeGenericMethod(new Type[]{ genericType });
            }
            cached_MethodInfo.Invoke(cached_FieldInfo.GetValue(instance), new object[]{ obj });
        }

    }

	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public sealed class ListLoadAttribute : Attribute{
		public ListLoadAttribute(Type typ){
            type = typ;
		}
        public Type type;
	}
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public sealed class IndexedLoadAttribute : Attribute{
		public IndexedLoadAttribute(int ld){
            loadOrder = ld;
		}
        public int loadOrder;
	}
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public sealed class RedirectLoadAttribute : Attribute{
		public RedirectLoadAttribute(Type defaultType){
            type = defaultType;
		}
        public Type type;
	}
    //Direct Value, only one allowed!
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public sealed class DirectLoadAttribute : Attribute{
		public DirectLoadAttribute(){
		}
	}
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public sealed class CustomLoadAttribute : Attribute{
		public CustomLoadAttribute(Action<VerbSequence, XmlNode, string> cla){
            private_actionSequence = cla;
		}
		public CustomLoadAttribute(Type type, string loadFromtring){
            loadFrom = type;
            loadFromField = loadFromtring;
		}
        public Action<VerbSequence, XmlNode, string> private_actionSequence;
        public Action<VerbSequence, XmlNode, string> actionSequence{
            get{
                if(private_actionSequence == null){
                    private_actionSequence = (Action<VerbSequence, XmlNode, string>)loadFrom.GetField(loadFromField).GetValue(null);
                }
                return private_actionSequence;
            }
        }
        public Type loadFrom;
        public string loadFromField;
	}
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public sealed class XmlStringStackAttribute : Attribute{
		public XmlStringStackAttribute(string loadFromtring){
            stringCHField = loadFromtring;
		}
        //public Type loadFromCache;
        public string stringCHField;
        private void getSCH(VerbSequence inst){
        }
        public void tryStack(VerbSequence inst, string str){
        }
	}
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public sealed class FixedLoadAttribute : Attribute{
		public FixedLoadAttribute(){
		}
		public FixedLoadAttribute(string alias_){
            alias = new string[]{ alias_ };
		}
		public FixedLoadAttribute(string[] alias_){
            alias = alias_;
		}
        public string[] alias;
	}
    //Warning! DefaultType ONLY works for direct primitives!
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public sealed class DefaultTypeAttribute : Attribute
	{
		public DefaultTypeAttribute(Type typ){
            type = typ;
		}
        public Type type;
	}

    /**public class VariableLoader {
        public static Vector3 NULL_vector3 = new Vector3(float.NaN, float.NaN, float.NaN);

        public float number = float.NaN;
        public Vector3 vector3 = new Vector3(float.NaN, float.NaN, float.NaN);
        public VerbSequence sequence;
        public ThingDef thingDef;
        public PawnKindDef pawnKindDef;
        public DamageDef damageDef;
        public HediffDef hediffDef;
        public BodyPartDef bodyPartDef;
        public MentalStateDef mentalStateDef;
        public IncidentDef incidentDef;
        public string text;
        public string variableNamed;

        public VerbSequence resolvedElement;
        public VerbSequence tryResolveAsVerbSequence(List<VerbScope> verbScopesParent, ScopeLeftType leftHand) {
            VerbSequence bf = func389979().registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
            resolvedElement = bf;
            return bf;
        }
        public VerbSequence func389979() {
            if (!float.IsNaN(number)) {
                return new VE_Number() { number = this.number };
            }
            if (sequence != null) {
                return sequence;
            }
            if (!float.IsNaN(vector3.x)) {
                VE_Vector3 v3 = new VE_Vector3() {
                    x = new VE_Number() { number = vector3.x },
                    y = new VE_Number() { number = vector3.y },
                    z = new VE_Number() { number = vector3.z }
                };
                return v3;
            }
            if (thingDef != null) {
                return new VE_ThingDef() { thingDef = this.thingDef };
            }
            if (damageDef != null) {
                return new VE_DamageDef() { damageDef = this.damageDef };
            }
            if (hediffDef != null) {
                return new VE_HediffDef() { hediffDef = this.hediffDef };
            }
            if (bodyPartDef != null) {
                return new VE_BodyPartDef() { bodyPartDef = this.bodyPartDef };
            }
            if (pawnKindDef != null) {
                return new VE_PawnKindDef() { pawnKindDef = this.pawnKindDef };
            }
            if (mentalStateDef != null) {
                return new VE_MentalStateDef() { mentalStateDef = this.mentalStateDef };
            }
            if (incidentDef != null) {
                return new VE_IncidentDef() { incidentDef = this.incidentDef };
            }
            if (text != null) {
                return new VE_String() { text = this.text };
            }
            throw new Exception("Invalid variable");
        }

    }**/


}
