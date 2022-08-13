using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbScript {

    /**public struct ValueAndSource{
        public object value;
        public VerbSequence source;
        public ValueAndSource(object val, VerbSequence sourc){
            value = val;
            source =  sourc;
        }

        public override bool Equals(object obj) {
            if(value == null){
                return obj == null;
            }
            return value.Equals(obj);
        }
        public override int GetHashCode() {
            return value == null? -1 : value.GetHashCode();
        }
        public override string ToString() {
            return value == null? "[null]" : value.ToString();
        }

    }**/

    public enum ReturnTypeStack{
        Undefined,
        Default,
        INTRAW
    }

    public static class SequenceHelper{

        public static object singular(this object ibj){
            if(ibj is IEnumerable<object> ienum){
                bool b = true;
                object fir = null;
                foreach(object obj in ienum){
                    if(b){
                        fir = obj;
                        b = false;
                    }
                }
                return fir;
                //object outt = ienum.First();
                //return outt;
            }
            return ibj;
        }
        public static IEnumerable<object> plural(this object ibj){
            if(ibj == null){
                Log.Error("Plural returned null!");
                yield break;
            }
        }
    }
    public abstract class VerbSequence {
        public static Dictionary<string, Type> SA_StringToType = new Dictionary<string, Type>();
        public static MethodInfo SA_OFXML_NGeneric = typeof(DirectXmlToObject).GetMethod("ObjectFromXml", BindingFlags.Public | BindingFlags.Static);
        public static Dictionary<Type, MethodInfo> SA_TypeToGeneric = new Dictionary<Type, MethodInfo>();
        public static MethodInfo getGenericMethod(Type type){
            if(SA_TypeToGeneric.TryGetValue(type, out MethodInfo methInfo)){
                return methInfo;
            }
            MethodInfo methRet = SA_OFXML_NGeneric.MakeGenericMethod(new Type[]{ type });
            SA_TypeToGeneric.Add(type, methRet);
            return methRet;
        }
        static VerbSequence(){
            SA_StringToType.Add("and", typeof(VC_GroupableAON_AND));
            SA_StringToType.Add("or", typeof(VC_GroupableAON_OR));
            SA_StringToType.Add("not", typeof(VC_GroupableAON_NOT));
            
            SA_StringToType.Add("true", typeof(VC_True));
            SA_StringToType.Add("yes", typeof(VC_True));

            SA_StringToType.Add("false", typeof(VC_False));
            SA_StringToType.Add("no", typeof(VC_False));
            
            SA_StringToType.Add("equal", typeof(VC_Equal));
            SA_StringToType.Add("equals", typeof(VC_Equal));
            
            SA_StringToType.Add("lesser", typeof(VC_Lesser));
            SA_StringToType.Add("less", typeof(VC_Lesser));
            
            SA_StringToType.Add("lesserequal", typeof(VC_LesserEqual));
            SA_StringToType.Add("lessequal", typeof(VC_LesserEqual));
            
            SA_StringToType.Add("greater", typeof(VC_Greater));
            SA_StringToType.Add("great", typeof(VC_Greater));
            
            SA_StringToType.Add("greaterequal", typeof(VC_GreaterEqual));
            SA_StringToType.Add("greatequal", typeof(VC_GreaterEqual));

            SA_StringToType.Add("hascurjobdef", typeof(VC_CurJobDef));
            SA_StringToType.Add("hascurjob", typeof(VC_CurJobDef));
            SA_StringToType.Add("hashediff", typeof(VC_HasHediff));
            SA_StringToType.Add("hasequipment", typeof(VC_HasEquipment));
            SA_StringToType.Add("hasequip", typeof(VC_HasEquipment));
            SA_StringToType.Add("equippedwith", typeof(VC_HasEquipment));
            SA_StringToType.Add("hastrait", typeof(VC_HasTrait));
            SA_StringToType.Add("isdrafted", typeof(VC_Drafted));
            SA_StringToType.Add("drafted", typeof(VC_Drafted));
            SA_StringToType.Add("ismoving", typeof(VC_Moving));
            SA_StringToType.Add("moving", typeof(VC_Moving));
            //SA_StringToType.Add("isdef", typeof(VC_ThingDef));
            SA_StringToType.Add("ishediffdef", typeof(VC_HediffDef));
            SA_StringToType.Add("isthingdef", typeof(VC_ThingDef));
            SA_StringToType.Add("israce", typeof(VC_ThingDef));
            SA_StringToType.Add("isracedef", typeof(VC_ThingDef));
            SA_StringToType.Add("isgender", typeof(VC_Gender));
            SA_StringToType.Add("issex", typeof(VC_Gender));
            SA_StringToType.Add("isanimal", typeof(VC_IsAnimal));
            SA_StringToType.Add("ishumanlike", typeof(VC_IsHumanlike));
            SA_StringToType.Add("ishuman", typeof(VC_IsHumanlike));
            SA_StringToType.Add("ismechanoid", typeof(VC_IsMechanoid));
            SA_StringToType.Add("ismecha", typeof(VC_IsMechanoid));

            SA_StringToType.Add("delay", typeof(VE_Delay));
            SA_StringToType.Add("wait", typeof(VE_Delay));
            SA_StringToType.Add("delayticks", typeof(VE_Delay));
            SA_StringToType.Add("waitticks", typeof(VE_Delay));
            
            //arithmatic
            SA_StringToType.Add("modulo", typeof(VE_Modulo));
            SA_StringToType.Add("mod", typeof(VE_Modulo));

            SA_StringToType.Add("addition", typeof(VE_Add));
            SA_StringToType.Add("add", typeof(VE_Add));
            SA_StringToType.Add("plus", typeof(VE_Add));
            
            SA_StringToType.Add("subtraction", typeof(VE_Subtract));
            SA_StringToType.Add("subtract", typeof(VE_Subtract));
            SA_StringToType.Add("sub", typeof(VE_Subtract));
            SA_StringToType.Add("minus", typeof(VE_Subtract));

            SA_StringToType.Add("multiply", typeof(VE_Multiply));
            SA_StringToType.Add("mult", typeof(VE_Multiply));
            SA_StringToType.Add("mul", typeof(VE_Multiply));

            SA_StringToType.Add("divide", typeof(VE_Divide));
            SA_StringToType.Add("div", typeof(VE_Divide));

            SA_StringToType.Add("min", typeof(VE_Min));
            SA_StringToType.Add("minimum", typeof(VE_Min));
            SA_StringToType.Add("max", typeof(VE_Max));
            SA_StringToType.Add("maximum", typeof(VE_Max));
            SA_StringToType.Add("rand", typeof(VE_Random));
            SA_StringToType.Add("random", typeof(VE_Random));
            SA_StringToType.Add("incident", typeof(VE_TriggerIncident));
            SA_StringToType.Add("triggerincident", typeof(VE_TriggerIncident));
            
            SA_StringToType.Add("cos", typeof(VE_Cosine));
            SA_StringToType.Add("cosine", typeof(VE_Cosine));
            SA_StringToType.Add("sin", typeof(VE_Sine));
            SA_StringToType.Add("sine", typeof(VE_Sine));
            SA_StringToType.Add("tan", typeof(VE_Tangent));
            SA_StringToType.Add("tangent", typeof(VE_Tangent));
            
            SA_StringToType.Add("passable", typeof(VC_Passable));
            SA_StringToType.Add("walkable", typeof(VC_Walkable));
            SA_StringToType.Add("standable", typeof(VC_Standable));
            
            SA_StringToType.Add("destroy", typeof(VE_Destroy));
            SA_StringToType.Add("spawndef", typeof(VE_SpawnDef));
            SA_StringToType.Add("spawn", typeof(VE_SpawnDef));
            SA_StringToType.Add("setterrain", typeof(VE_SetTerrain));
            
            SA_StringToType.Add("if", typeof(VE_If));
            SA_StringToType.Add("elseif", typeof(VE_If));
            SA_StringToType.Add("else_if", typeof(VE_If));
            SA_StringToType.Add("else if", typeof(VE_If));
            SA_StringToType.Add("else", typeof(VE_BracketBlock));
            SA_StringToType.Add("while", typeof(VE_While));
            
            SA_StringToType.Add("damage", typeof(VE_ThingDamage));
            SA_StringToType.Add("damagething", typeof(VE_ThingDamage));
            SA_StringToType.Add("thingdamage", typeof(VE_ThingDamage));
            SA_StringToType.Add("celldamage", typeof(VE_CellDamage));
            SA_StringToType.Add("explode", typeof(VE_CellDamage));
            
            //enumerable
            SA_StringToType.Add("script", typeof(VE_CallScript));
            SA_StringToType.Add("returnlast", typeof(VE_ReturnLast));
            SA_StringToType.Add("executereturnlast", typeof(VE_ReturnLast));
            SA_StringToType.Add("selecthighest", typeof(VE_SelectHighest));
            SA_StringToType.Add("selectrandom", typeof(VE_SelectRandom));
            SA_StringToType.Add("highest", typeof(VE_SelectHighest));
            SA_StringToType.Add("pawnsinmap", typeof(VE_PawnsInMap));
            SA_StringToType.Add("hostilepawnsinmap", typeof(VE_HostilePawnsInMap));
            SA_StringToType.Add("allhediffs", typeof(VE_AllHediffs));
            SA_StringToType.Add("allfactions", typeof(VE_AllFactions));
            SA_StringToType.Add("thingsat", typeof(VE_ThingsInRadius));
            SA_StringToType.Add("thingsinradius", typeof(VE_ThingsInRadius));
            SA_StringToType.Add("cellsinradius", typeof(VE_CellsInRadius));
            SA_StringToType.Add("triggercomp", typeof(VE_CompTrigger));
            SA_StringToType.Add("activatecomp", typeof(VE_CompTrigger));
            SA_StringToType.Add("comptrigger", typeof(VE_CompTrigger));
            SA_StringToType.Add("compactivate", typeof(VE_CompTrigger));
            SA_StringToType.Add("teleport", typeof(VE_Teleport));
            SA_StringToType.Add("train", typeof(VE_Train));

            SA_StringToType.Add("number", typeof(VE_Number));
            SA_StringToType.Add("num", typeof(VE_Number));
            
            SA_StringToType.Add("bodypartdef", typeof(VE_BodyPartDef));
            SA_StringToType.Add("thingdef", typeof(VE_ThingDef));
            SA_StringToType.Add("damagedef", typeof(VE_DamageDef));
            SA_StringToType.Add("incidentdef", typeof(VE_IncidentDef));
            SA_StringToType.Add("hediffdef", typeof(VE_HediffDef));
            SA_StringToType.Add("terraindef", typeof(VE_TerrainDef));
            SA_StringToType.Add("pawnkinddef", typeof(VE_PawnKindDef));
            SA_StringToType.Add("mentalstatedef", typeof(VE_MentalStateDef));
            SA_StringToType.Add("trainabledef", typeof(VE_TrainableDef));
            SA_StringToType.Add("string", typeof(VE_String));
            SA_StringToType.Add("text", typeof(VE_String));
            
            SA_StringToType.Add("loadlocal", typeof(VEC_LoadVariableLocal));
            SA_StringToType.Add("load", typeof(VS_LoadVariable));
            SA_StringToType.Add("savelocal", typeof(VEC_SaveVariableLocal));
            SA_StringToType.Add("save", typeof(VE_SaveVariable));
            SA_StringToType.Add("saveas", typeof(VE_SaveVariable));
            SA_StringToType.Add("print", typeof(VE_Print));
            
            SA_StringToType.Add("savething", typeof(VE_SaveVariableThing));
            SA_StringToType.Add("savehediff", typeof(VE_SaveVariableHediff));
            SA_StringToType.Add("savefloat", typeof(VE_SaveVariableFloat));
            SA_StringToType.Add("savevector3", typeof(VE_SaveVariableVector3));
            SA_StringToType.Add("savestring", typeof(VE_SaveVariableString));

            SA_StringToType.Add("inmentalstate", typeof(VC_InMentalState));
            SA_StringToType.Add("setmentalstate", typeof(VE_MentalState));
            SA_StringToType.Add("setfaction", typeof(VE_SetFaction));
            SA_StringToType.Add("givehediff", typeof(VE_GiveHediff));
            SA_StringToType.Add("applyhediff", typeof(VE_GiveHediff));
            SA_StringToType.Add("removetrait", typeof(VE_RemoveTrait));
            SA_StringToType.Add("addtrait", typeof(VE_AddTrait));
            SA_StringToType.Add("vector3", typeof(VE_Vector3));
            SA_StringToType.Add("vector", typeof(VE_Vector3));
            SA_StringToType.Add("rotate", typeof(VE_Rotate));
            SA_StringToType.Add("normalize", typeof(VE_Normalize));
            SA_StringToType.Add("distanceynormalized", typeof(VE_DistanceYNormalized));
            SA_StringToType.Add("localtarget", typeof(VS_LocalTarget));
            SA_StringToType.Add("aimtarget", typeof(VS_LocalTarget));
            SA_StringToType.Add("extraaimtarget", typeof(VE_ExtraLocalTarget));
            
            SA_StringToType.Add("exists", typeof(VC_NotNull));
            SA_StringToType.Add("exist", typeof(VC_NotNull));
            SA_StringToType.Add("notnull", typeof(VC_NotNull));
            SA_StringToType.Add("isnotnull", typeof(VC_NotNull));
            
            SA_StringToType.Add("isnull", typeof(VC_IsNull));

            SA_StringToType.Add("istype", typeof(VC_IsType));
            SA_StringToType.Add("topscope", typeof(VS_TopScope));
            SA_StringToType.Add("prevscope", typeof(VS_PrevScope));
            SA_StringToType.Add("prevprevscope", typeof(VS_PrevPrevScope));
            SA_StringToType.Add("rootscope", typeof(VS_RootScope));
            
            SA_StringToType.Add("filtersequence", typeof(VE_FilterSequence));
            SA_StringToType.Add("filter", typeof(VE_FilterSequence));
            SA_StringToType.Add("foreach", typeof(VE_Foreach));

            //MISC
            SA_StringToType.Add("lightning", typeof(VE_SummonLightning));

            //Reflection
            SA_StringToType.Add("getfield", typeof(VE_FieldPropertyChain));
            SA_StringToType.Add("getfields", typeof(VE_FieldPropertyChain));
            SA_StringToType.Add("getproperty", typeof(VE_FieldPropertyChain));
            SA_StringToType.Add("getproperties", typeof(VE_FieldPropertyChain));
            SA_StringToType.Add("setfield", typeof(VE_FieldPropertyChainSetter));
            SA_StringToType.Add("setfields", typeof(VE_FieldPropertyChainSetter));
            SA_StringToType.Add("setproperty", typeof(VE_FieldPropertyChainSetter));
            SA_StringToType.Add("setproperties", typeof(VE_FieldPropertyChainSetter));
            
            SA_StringToType.Add("mode_ignore", typeof(VEC_Ignore));
            SA_StringToType.Add("mode_chase", typeof(VEC_Chase));
            SA_StringToType.Add("mode_fire", typeof(VEC_Fire));

            //alias
            SA_StringToType.Add("destroyat", typeof(VEC_DestroyAt)); //INTO Destroy
            SA_StringToType.Add("repeat", typeof(VEC_Repeat)); //INTO while

        }
        public static Dictionary<Type, bool> SA_AliasCheck = new Dictionary<Type, bool>();
        public bool func747193(){
            return GetType().GetMethod("ResolveAlias").DeclaringType != typeof(VerbSequence);
        }
        public bool hasAlias(){
            return SA_AliasCheck.getInitSafe(GetType(), func747193);
        }
        public virtual VerbSequence ResolveAlias(){
            return this;
        }
        public static string func3298(XmlNode xml){
            if(xml.ChildNodes.Count == 0){
                return null;
            }
            return xml.FirstChild.Value;
        }

        public void applyNext(ref VerbSequence lastVerbSequence, HashSet<FieldLoader> completeFieldLoader, FieldLoader fLoader, Type defaultType, XmlNode xmlNode, bool list = false){
            completeFieldLoader.Add(fLoader);
            VerbSequence verbSeq = (VerbSequence)getGenericMethod(defaultType).Invoke(null, new object[] { xmlNode, null });
            if(lastVerbSequence != null && lastVerbSequence.applyNextOverride(xmlNode, verbSeq)){
                //Log.Warning("APPLY NEXT OVERRIDE " + xmlNode.InnerText);
            }else{
                if(list){
                    fLoader.applyElement(this, verbSeq);
                }else{
                    fLoader.apply(this, verbSeq);
                } 
            }
            lastVerbSequence = verbSeq;
        }
        public virtual bool applyNextOverride(XmlNode origin, VerbSequence nextElement){
            return false;
        }
        //Custom XML Loader
		public void LoadDataFromXmlCustom(XmlNode xmlRoot){
            bool LOGNOW = false;//xmlRoot.Name.Contains("isNull");
            TypeLoader tLoad = TypeLoader.GetTypeLoader(this.GetType());
            if(LOGNOW){
                Log.Warning("Generating " + this + " nodename " + xmlRoot.Name);
            }
            HashSet<FieldLoader> completeFieldLoader = new HashSet<FieldLoader>();
            VerbSequence lastVerbSequence = null;

            if(tLoad.directLoader != null && !func3298(xmlRoot).NullOrEmpty()){
                if(LOGNOW){
                    Log.Warning("PreParam 0 " + func3298(xmlRoot));
                }
                if(tLoad.directLoader.customDirect != null){
                    if(LOGNOW){
                        Log.Warning("PreParam LoadCustom ");
                    }
                    tLoad.directLoader.customDirect(this, xmlRoot, func3298(xmlRoot));
                }else{
                    switch(tLoad.directLoader.loadType){
                        case 0:{
                            string param = func3298(xmlRoot);
                            if(LOGNOW){
                                Log.Warning("Param O1 " + param);
                            }
                            completeFieldLoader.Add(tLoad.directLoader);
                            tLoad.directLoader.apply(this, ParseHelper.FromString(param, tLoad.directLoader.defaultLoadType));
                            break;
                        }
                        case 1:{
                            string param = func3298(xmlRoot);
                            if(LOGNOW){
                                Log.Warning("Param O2 " + param);
                            }
                            completeFieldLoader.Add(tLoad.directLoader);
                            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, tLoad.directLoader.fieldName, param, null, null);
                            break;
                        }
                        case 2:{
                            //Log.Error("Invalid Scope");
                            break;
                        }
                    }
                }
            }else if(tLoad.redirectLoader != null && !func3298(xmlRoot).NullOrEmpty()){
                if(LOGNOW){
                    Log.Warning("PreParam 1 " + func3298(xmlRoot));
                }
                applyNext(ref lastVerbSequence, completeFieldLoader, tLoad.redirectLoader, tLoad.redirectLoader.defaultLoadType, xmlRoot);
            }
            else{
                int indexNow = 0;
                foreach(XmlNode childNode in xmlRoot.ChildNodes){
                    string childName = childNode.Name.ToLower();
                    if(tLoad.fixedStringToFieldLoader.TryGetValue(childName, out FieldLoader fDestination)){
                        if(fDestination.defaultLoadType != null){
                            if(childNode.FirstChild.Name.Equals("#text")){
                                switch(fDestination.loadType){
                                    case 0:{
                                        string param = func3298(childNode);
                                        if(LOGNOW){
                                            Log.Warning("Param B1 " + param);
                                        }
                                        completeFieldLoader.Add(fDestination);
                                        fDestination.apply(this, ParseHelper.FromString(param, fDestination.defaultLoadType));
                                        break;
                                    }
                                    case 1:{
                                        string param = func3298(childNode);
                                        if(LOGNOW){
                                            Log.Warning("Param B2 " + param);
                                        }
                                        completeFieldLoader.Add(fDestination);
                                        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, fDestination.fieldName, param, null, null);
                                        break;
                                    }
                                    case 2:{
                                        if(LOGNOW){
                                            Log.Warning("Node push 0 " + childNode.Name + " / " + childNode.Value);
                                        }
                                        applyNext(ref lastVerbSequence, completeFieldLoader, fDestination, fDestination.defaultLoadType, childNode);
                                        break;
                                    }
                                }
                            }else{
                                string childNameLower = childNode.FirstChild.Name.ToLower();
                                if(SA_StringToType.TryGetValue(childNameLower, out Type type)){
                                    if(LOGNOW){
                                        Log.Warning("Node push 1 " + childNode.Name + " / " + childNode.Value);
                                    }
                                    applyNext(ref lastVerbSequence, completeFieldLoader, fDestination, type, childNode.FirstChild);
                                }else{
                                    Log.Error("Error code F136AB3: Report this to the developer. " + childNameLower + " / "+ xmlRoot.OuterXml);
                                }
                            }
                        }else if(fDestination.customDirect != null){
                            fDestination.customDirect(this, childNode, "");
                        }else if(childNode.ChildNodes.Count == 1){
                            string childNameLower = childNode.FirstChild.Name.ToLower();
                            if(SA_StringToType.TryGetValue(childNameLower, out Type type)){
                                if(LOGNOW){
                                    Log.Warning("Node push 2 " + childNode.Name + " / " + childNode.Value);
                                }
                                applyNext(ref lastVerbSequence, completeFieldLoader, fDestination, type, childNode.FirstChild);
                            }else{
                                Log.Error("Error code F136AB2: Report this to the developer. " + childNameLower + " / " + xmlRoot.OuterXml);
                            }
                        }else{
                            Log.Error("Error code 816A3D8: Report this to the developer. " + xmlRoot.OuterXml);
                        }
                    }else if(SA_StringToType.TryGetValue(childName, out Type type)){
                        bool indexedFound = false;
                        if(tLoad.listedLoader.Count > indexNow){
                            FieldLoader fDestination2 = tLoad.listedLoader[indexNow];
                            while(completeFieldLoader.Contains(fDestination2)){
                                indexNow += 1;
                                if(indexNow == tLoad.listedLoader.Count){
                                    fDestination2 = null;
                                    break;
                                }else{
                                    fDestination2 = tLoad.listedLoader[indexNow];
                                }
                            }
                            if(fDestination2 != null){
                                indexedFound = true;
                                applyNext(ref lastVerbSequence, completeFieldLoader, fDestination2, type, childNode);
                                if(LOGNOW){
                                    Log.Warning("Node push 3 " + childNode.Name + " / " + childNode.Value);
                                }
                            }
                        }
                        if(!indexedFound){
                            FieldLoader fDestination2 = tLoad.listDestination;
                            applyNext(ref lastVerbSequence, completeFieldLoader, fDestination2, type, childNode, true);
                            if(LOGNOW){
                                Log.Warning("Node push 4 " + childNode.Name + " / " + childNode.Value);
                            }
                        }
                    }else{
                        Log.Warning("Unknown type " + childName + " /parent/ " + xmlRoot.Name);
                    }
                }
            }

		}

        //Static
        public static Dictionary<stringCH, VerbSequence> verbEffectUniqueInstance = new Dictionary<stringCH, VerbSequence>();
        public static StringBuilder SA_StringBuilder = new StringBuilder();
        public const long ID_Gap = uint.MaxValue;
        public struct LongInt{
            public long key1;
            public int key2;
            public LongInt(long a, int b){
                key1 = a;
                key2 = b;
            }
            public override int GetHashCode() {
                return hashShift((int)(key1 >> 32)) ^ hashShift((int)(key1 >> 0)) ^ hashShift(key2);
            }
            public override bool Equals(object obj) {
                return obj is LongInt obs && obs.key1 == key1 && obs.key2 == key2;
            }
            public static int hashShift(int h2) {
                uint h = (uint)h2;
                h ^= (h >> 20) ^ (h >> 12);
                return (int)(h ^ (h >> 7) ^ (h >> 4));
            }
        }
        public static Dictionary<LongInt, Dictionary<Type, long>> SA_TypeToUniqueID = new Dictionary<LongInt, Dictionary<Type, long>>();
        public static long SA_currentTypeID = 0;
        public static long nextUID(){
            SA_currentTypeID += ID_Gap;
            return SA_currentTypeID;
        }
        public long typeID(LongInt groupIndexIfAny){
            if(!SA_TypeToUniqueID.ContainsKey(groupIndexIfAny)){
                SA_TypeToUniqueID.Add(groupIndexIfAny, new Dictionary<Type, long>());
            }
            Type type = this.GetType();
            if(!SA_TypeToUniqueID[groupIndexIfAny].ContainsKey(type)){
                SA_TypeToUniqueID[groupIndexIfAny].Add(type, nextUID());
            }
            return SA_TypeToUniqueID[groupIndexIfAny][type];
        }
        //Cache
        public static int SLCacheIDNow = -1;
        public static bool SLResultCache = false;
        private IEnumerable<object> cachedResultPrivate; //This is hashset? or List?
        public int cacheID;
        public virtual ICollection<object> CreateResultCacheCollection(){
            return FreePool<List<object>>.next();
        }
        public virtual bool ShouldUseResultCache(){
            return false;
        }
        public static void func1287390(ref IEnumerable<object> CRP){
            if(CRP is HashSet<object> hs){
                hs.Clear();
                FreePool<HashSet<object>>.free(hs);
            }else if(CRP is List<object> l){
                l.Clear();
                FreePool<List<object>>.free(l);
            }
            CRP = null;
        }
        public IEnumerable<object> cachedResult{
            get{
                if(cacheID == SLCacheIDNow){
                    return cachedResultPrivate;
                }
                return null;
            }set{
                if(cachedResultPrivate != null){
                    func1287390(ref cachedResultPrivate);
                }
                cachedResultPrivate = value;
                cacheID = SLCacheIDNow;
            }
        }
        public static void ClearCache(){
            SLCacheIDNow += 1;
        }
        //Utility
        public bool isIdentitical(VerbSequence otherFilter) {
            return uniqueID == otherFilter.uniqueID;
            //return otherFilter.GetType() == this.GetType();
        }        
        //If A is in the same group as B, and B is in the same group as C, then A MUST be in the same group as C
        public virtual bool isSameGroup(VerbSequence otherFilter) {
            long group = CT_groupingHash();
            return group != -1 && group == otherFilter.CT_groupingHash();
        }
        public static List<List<VerbScope>> freeVerbScopeLists = new List<List<VerbScope>>();
        public static List<VerbScope> nextVerbScopeList(){
            if(freeVerbScopeLists.Count > 0){
                return freeVerbScopeLists.Pop();
            }
            return new List<VerbScope>();
        }
        public static void free(List<VerbScope> verbScope){
            verbScope.Clear();
            freeVerbScopeLists.Add(verbScope);
        }
        //ID        
        public List<VerbScope> leftHandParentScopes = new List<VerbScope>();//order matters too!
        public int scopeSequenceIndex;
        public void calculateScopeSequenceIndex(){
            scopeSequenceIndex = UniqueSequence<VerbScope>.registerAndReturnIndex(leftHandParentScopes);
        }
        private stringCH uniqueIDPrivate;
        public stringCH calculateStringID(){
            SA_StringBuilder.Clear();
            /**for(int i = 0; i < this.leftHandParentScopes.Count; i++){
                leftHandParentScopes[i].appendID();
            }**/
            SA_StringBuilder.Append("[ScopeIndex" + this.scopeSequenceIndex + "]");
            appendID();
            return SA_StringBuilder.ToString().CH();
        }
        public virtual void appendID(){
            SA_StringBuilder.Append("[" + this.GetType().Name + "]");
        }
        public stringCH uniqueID{
            get{
                if(uniqueIDPrivate.text == null){
                    uniqueIDPrivate = calculateStringID();
                }
                return uniqueIDPrivate;
            }
        }
        //Initialize
        public virtual int QD_quickFilterIDInt(){
            return uniqueSubIDFromContent();
        }
        public virtual void RegisterAllTypes(VerbRootQD destination){
            destination.func2394(GetType(), true).Add(this.QD_quickFilterIDInt());
        }
        public virtual VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            if(hasAlias()){
                return this.ResolveAlias().registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
            }
            this.leftHandParentScopes.AddRange(verbScopesParent);
            calculateScopeSequenceIndex();
            VerbSequence veOut;
            if(!verbEffectUniqueInstance.TryGetValue(uniqueID, out veOut)){
                verbEffectUniqueInstance.Add(uniqueID, this);
            }else{
                return veOut;
            }
            return this;
        }
        //Tree Sort
        public virtual long PG_groupIndex(){
            return 0;
        }
        public virtual long CT_groupingHash(){//return -1 if invalid, do not use 0 ~ 20000
            return -1;
        }
        public virtual int uniqueSubIDFromContent(){
            return 0;
        }
        //Evaluate
        public virtual bool shouldRewindExecuteStack(ExecuteStackContext esc){
            return false;
        }
        public virtual bool shouldSkipSubExecuteStack(ExecuteStackContext esc){
            return false;
        }
        /**public virtual int stateEvaluate(ExecuteStackContext context){
            if(SLResultCache){
                object objC = cachedResult;
                if(objC == null){
                    objC = cachedResult = (int)evaluate(context).singular();
                } 
                return (int)objC;
            }else{
                return (int)evaluate(context).singular();
            }
        }
        public virtual int stateQuickEvaluate(ExecuteStackContext context){
            if(SLResultCache){
                object objC = cachedResult;
                if(objC == null){
                    objC = cachedResult = (int)this.quickEvaluate(context).singular();
                } 
                return (int)objC;
            }else{
                object outt = this.quickEvaluate(context).singular();
                if(outt is int abc){
                    return abc;
                }else{
                    Log.Warning("Output is not int " + outt);
                    return 0;
                }
            }
        }**/
        public virtual IEnumerable<object> evaluate(ExecuteStackContext context){
            throw new NotImplementedException("Not implemented for " + this.GetType());
        }
        public virtual int scriptSequenceGroup(ExecuteStackContext context){
            return 0;
        }
        public virtual int scriptIndexMax(int scriptSequenceGroupSubIndex){
            return 0;
        }
        public virtual VerbSequence getSequenceAt(int index, int subSequenceGroupIndex){
            return null;
        }
        //Set Sort
        public GroupState func198273(VerbSequence vegaon){
            return groupStateCompareSingular(vegaon);
        }
        protected virtual GroupState groupStateCompareSingular(VerbSequence vegaon){
            if(vegaon == null){
                Log.Warning("Comparing with null! this shouldnt happen");
            }
            return (this.isIdentitical(vegaon)) ? GroupState.Same : GroupState.NoOverlap;
        }
        public void clearParentChild(){
            parentSetPrivate?.Clear();
            parentSetPrivate = null;
            childSetPrivate?.Clear();
            childSetPrivate = null;
        }
        public static HashSet<VerbSequence> SA_EmptySet = new HashSet<VerbSequence>();
        private HashSet<VerbSequence> parentSetAllPrivate;
        private HashSet<VerbSequence> childSetAllPrivate;
        private HashSet<VerbSequence> parentSetPrivate;
        private HashSet<VerbSequence> childSetPrivate;
        public static Dictionary<VerbSequence, HashSet<VerbSequence>> SA_parentSetTemp = new Dictionary<VerbSequence, HashSet<VerbSequence>>();
        public static Dictionary<VerbSequence, HashSet<VerbSequence>> SA_childSetTemp = new Dictionary<VerbSequence, HashSet<VerbSequence>>();
        public HashSet<VerbSequence> parentSet(bool readOnly = false){
            if(parentSetPrivate == null){
                if(readOnly){
                    return SA_EmptySet;
                }
                parentSetPrivate = new HashSet<VerbSequence>();
            }
            return parentSetPrivate;
        }
        public HashSet<VerbSequence> childSet(bool readOnly = false){
            if(childSetPrivate == null){
                if(readOnly){
                    return SA_EmptySet;
                }
                childSetPrivate = new HashSet<VerbSequence>();
            }
            return childSetPrivate;
        }
        public HashSet<VerbSequence> parentSetTotal(bool readOnly = false){
            if(parentSetAllPrivate == null){
                if(readOnly){
                    return SA_EmptySet;
                }
                parentSetAllPrivate = new HashSet<VerbSequence>();
            }
            return parentSetAllPrivate;
        }
        public HashSet<VerbSequence> childSetTotal(bool readOnly = false){
            if(childSetAllPrivate == null){
                if(readOnly){
                    return SA_EmptySet;
                }
                childSetAllPrivate = new HashSet<VerbSequence>();
            }
            return childSetAllPrivate;
        }
        public void registerAsChild(VerbSequence veg){
            registerAsChild(veg, SA_GetChildSet, SA_GetParentSet);
        }
        public void deregisterChild(VerbSequence veg){
            deregisterChild(veg, SA_GetChildSet, SA_GetParentSet);
        }
        public void registerAsChild(VerbSequence veg, Func<VerbSequence, HashSet<VerbSequence>> child, Func<VerbSequence, HashSet<VerbSequence>> parent){
            parent(veg).Add(this);
            child(this).Add(veg);
            //veg.parentSet().Add(this);
            //this.childSet().Add(veg);
            if(veg == this){
                Log.Error("Adding self as child node!");
            }
        }
        public void deregisterChild(VerbSequence veg, Func<VerbSequence, HashSet<VerbSequence>> child, Func<VerbSequence, HashSet<VerbSequence>> parent){
            if(!parent(veg).Remove(this)){
                Log.Error("Desync not removed " + this + " / " + veg);
            }
            child(this).Remove(veg);
        }
        public static HashSet<VerbSequence> SA_enteredNodes = new HashSet<VerbSequence>();
        public static bool CheckAndRegister(VerbSequence aon){
            if(!SA_enteredNodes.Contains(aon)){
                SA_enteredNodes.Add(aon);
                return true;
            }
            return false;
        }
        public static void ClearRegister(){
            SA_enteredNodes.Clear();
        }
        public static HashSet<VerbSequence> SA_PostInteractRemove = new HashSet<VerbSequence>();
        //TODO order of effects affect performance?
        public static HashSet<VerbSequence> topLayer(List<VerbSequence> testV){
		    HashSet<VerbSequence> topV = new HashSet<VerbSequence>();
		    for(int i = 0; i < testV.Count; i++){
                ClearRegister();
			    VerbSequence v3 = testV[i];
			    bool added = false;
			    foreach(VerbSequence compareN in topV){
				    added |= compareN.tryParentChildSortInstance_NodeInteract(v3, topV, SA_GetChildSet, SA_GetParentSet, SA_GroupState_Compute);
			    }
                if(SA_PostInteractRemove.Count > 0){
                    foreach(VerbSequence ave in SA_PostInteractRemove){
                        topV.Remove(ave);
                    }
                    SA_PostInteractRemove.Clear();
                }
			    if(!added){
				    topV.Add(v3);
			    }
		    }
		    foreach(VerbSequence v3 in testV){
                ClearRegister();
                v3.topDownRegister(v3);
            }
            return topV;
        }
        public static HashSet<VerbSequence> topLayerTemp(List<VerbSequence> testV){
            SA_parentSetTemp.Clear();
            SA_childSetTemp.Clear();
		    HashSet<VerbSequence> topV = new HashSet<VerbSequence>();
		    for(int i = 0; i < testV.Count; i++){
                ClearRegister();
			    VerbSequence v3 = testV[i];
			    bool added = false;
			    foreach(VerbSequence compareN in topV){
				    added |= compareN.tryParentChildSortInstance_NodeInteract(v3, topV, SA_GetChildSetTopCheckOnly, SA_GetParentSetTopCheckOnly, SA_GroupState_Cache);
			    }
                if(SA_PostInteractRemove.Count > 0){
                    foreach(VerbSequence ave in SA_PostInteractRemove){
                        topV.Remove(ave);
                    }
                    SA_PostInteractRemove.Clear();
                }
			    if(!added){
				    topV.Add(v3);
			    }
		    }
            return topV;
        }
        public void topDownRegister(VerbSequence add){
            foreach(VerbSequence child in childSet(true)){
                if(CheckAndRegister(child)){
                    add.childSetTotal(false).Add(child);
                    child.parentSetTotal(false).Add(add);
                    child.topDownRegister(add);
                }
            }
        }
        public static Func<VerbSequence, HashSet<VerbSequence>> SA_GetChildSet = delegate (VerbSequence ve) {
            return ve.childSet();
        };
        public static Func<VerbSequence, HashSet<VerbSequence>> SA_GetParentSet = delegate (VerbSequence ve) {
            return ve.parentSet();
        };
        public static Func<VerbSequence, HashSet<VerbSequence>> SA_GetChildSetTopCheckOnly = delegate (VerbSequence ve) {
            HashSet<VerbSequence> veS;
            if(VerbSequence.SA_childSetTemp.TryGetValue(ve, out veS)){
                return veS;
            }
            veS = new HashSet<VerbSequence>();
            VerbSequence.SA_childSetTemp.Add(ve, veS);
            return veS;
        };
        public static Func<VerbSequence, HashSet<VerbSequence>> SA_GetParentSetTopCheckOnly = delegate (VerbSequence ve) {
            HashSet<VerbSequence> veS;
            if(VerbSequence.SA_parentSetTemp.TryGetValue(ve, out veS)){
                return veS;
            }
            veS = new HashSet<VerbSequence>();
            VerbSequence.SA_parentSetTemp.Add(ve, veS);
            return veS;
        };
        public static Func<VerbSequence, VerbSequence, GroupState> SA_GroupState_Compute = delegate (VerbSequence veLeft, VerbSequence veRight) {
            return GroupStateUtility.compareTwo(veLeft, veRight);
        };
        public static Func<VerbSequence, VerbSequence, GroupState> SA_GroupState_Cache = delegate (VerbSequence veLeft, VerbSequence veRight) {
            if(veLeft.childSetTotal(true).Contains(veRight)){
                return GroupState.BinA;
            }
            if(veLeft.parentSetTotal(true).Contains(veRight)){
                return GroupState.AinB;
            }
            return GroupState.PartialOverlap;
        };
        //return true if it became a child
        public bool tryParentChildSortInstance_NodeInteract<T>(T newlyAdded, HashSet<T> topList, Func<VerbSequence, HashSet<VerbSequence>> child, Func<VerbSequence, HashSet<VerbSequence>> parent, Func<VerbSequence, VerbSequence, GroupState> groupStateCompare) where T : VerbSequence{
            bool becameChild = false;
            if(CheckAndRegister(this)){
                GroupState groupStateNow = groupStateCompare(newlyAdded, this);//GroupStateUtility.compareTwo(newlyAdded, this);//newlyAdded.groupStateCompare(this);
                //GroupStateUtility.compareTwo(this, newlyAdded);
                switch(groupStateNow){
                    case GroupState.BinA:{
                        HashSet<VerbSequence> parentSaveSet = nextHashSet();
                        parentSaveSet.AddRange(parent(this));
                        if(parentSaveSet.Count == 0){
                            //topList.Remove((T)this);
                            SA_PostInteractRemove.Add(this);
                        }else{
                            foreach(VerbSequence parentElement in parentSaveSet){
                                GroupState gss = groupStateCompare(newlyAdded, parentElement);//GroupStateUtility.compareTwo(newlyAdded, parentElement);//newlyAdded.groupStateCompare(parentElement);
                                //GroupStateUtility.compareTwo(parentElement, newlyAdded);
                                switch(gss){
                                    case GroupState.AinB:{
                                        parentElement.deregisterChild(this, child, parent);//transfer child to this
                                        newlyAdded.registerAsChild(this, child, parent);
                                        break;
                                    }
                                    case GroupState.BinA:{
                                        Log.Error("This should never happen BinA BinA " + newlyAdded.uniqueID + " / " + parentElement.uniqueID);
                                        break;
                                    }
                                    case GroupState.PartialOverlap:{
                                        break;
                                    }
                                    case GroupState.Same:{
                                        Log.Error("This should never happen BinA Type_Same" + newlyAdded.uniqueID + " / " + parentElement.uniqueID);
                                        break;
                                    }
                                    case GroupState.NoOverlap:{//is this really the same as Partial OVerlap
                                        //Log.Error("This should never happen BinA Type_NoOverlap" + newlyAdded.uniqueID + " / " + parentElement.uniqueID);
                                        break;
                                    }
                                }
                            }
                        }
                        free(parentSaveSet);
                        newlyAdded.registerAsChild(this, child, parent);
                        break;
                    }
                    case GroupState.AinB:{
                        HashSet<VerbSequence> childSaveSet = nextHashSet();
                        childSaveSet.AddRange(child(this));
                        foreach(VerbSequence childElement in childSaveSet){
                            becameChild |= childElement.tryParentChildSortInstance_NodeInteract<T>(newlyAdded, topList, child, parent, groupStateCompare);
                        }
                        free(childSaveSet);

                        if(!becameChild){
                            registerAsChild(newlyAdded, child, parent);
                            becameChild = true;
                        }
                        break;
                    }
                    case GroupState.PartialOverlap:{
                        HashSet<VerbSequence> childSaveSet = nextHashSet();
                        childSaveSet.AddRange(child(this));
                        foreach(VerbSequence childElement in childSaveSet){
                            if(childElement.tryParentChildSortInstance_NodeInteract<T>(newlyAdded, topList, child, parent, groupStateCompare)){
                                Log.Error("This should never happen Type_PartialOverlap " + newlyAdded.uniqueID + " / " + this.uniqueID);
                            }
                        }
                        free(childSaveSet);
                        //CHECK CHILD NODES
                        break;
                    }
                    case GroupState.NoOverlap:{
                        break;
                    }
                    case GroupState.Same:{
                        Log.Error("This should never happen Type_Same " + newlyAdded.uniqueID + " / " + this.uniqueID);
                        //ERROR
                        break;
                    }
                }
            }
            return becameChild;
        }
        public static List<HashSet<VerbSequence>> SA_FreeHSV = new List<HashSet<VerbSequence>>();
        public static HashSet<VerbSequence> nextHashSet(){
            if(SA_FreeHSV.Count > 0){
                return SA_FreeHSV.Pop();
            }
            return new HashSet<VerbSequence>();
        }
        public static void free(HashSet<VerbSequence> hs){
            hs.Clear();
            SA_FreeHSV.Add(hs);
        }
    }
    
}
