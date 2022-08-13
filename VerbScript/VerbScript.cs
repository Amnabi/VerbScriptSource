using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using System.Xml;
using System.Reflection;

namespace VerbScript {

    [StaticConstructorOnStartup]
    public static class VerbScriptLoader{
        static VerbScriptLoader(){
            VerbData.preEnsureAllUnique();
            VerbData.ensureAllUnique();
            for(int j = 0; j < 1; j++){
                //List<VerbData> vb = new List<VerbData>();
                //Log.Warning("IIA " + VerbData.SA_ThingDefIndexToUncompiled.Count);
                List<VerbData> vsAlt = VerbData.SA_ThingDefIndexToUncompiled.ContainsKey(-1)? VerbData.SA_ThingDefIndexToUncompiled[-1] : new List<VerbData>();
                foreach(int i in VerbData.SA_ThingDefIndexToUncompiled.Keys){
                    //Log.Warning("Compiling " + i);
                    List<VerbData> compiled = VerbData.SA_ThingDefIndexToUncompiled[i];
                    if(i != -1){
                        compiled.AddRange(vsAlt);
                    }
                    VerbData.SA_ThingDefIndexToTree.Add(i, VerbData.initializeAll(compiled, j));
                }
                
            }
            if(!VerbData.SA_ThingDefIndexToTree.ContainsKey(-1)){
                //Log.Warning("Adding Default tree");
                VerbData.SA_ThingDefIndexToTree.Add(-1, VerbSequenceRoot.nextACT());
            }
        }
    }

    public class VerbScript {
        public static Dictionary<string, VerbScript> scriptByName = new Dictionary<string, VerbScript>();
        public string scriptName;
        public void setName(string name){
            scriptName = name;
            scriptByName.Add(name, this);
        }
        public List<VerbSequence> sequence = new List<VerbSequence>();
        public void applyNext(ref VerbSequence lastVerbSequence, Type defaultType, XmlNode xmlNode){
            VerbSequence verbSeq = (VerbSequence)VerbSequence.getGenericMethod(defaultType).Invoke(null, BindingFlags.Public | BindingFlags.Static, null, new object[] { xmlNode, null }, null);
            if(lastVerbSequence != null && lastVerbSequence.applyNextOverride(xmlNode, verbSeq)){
            }else{
                sequence.Add(verbSeq);
            }
            lastVerbSequence = verbSeq;
        }
		public void LoadDataFromXmlCustom(XmlNode xmlRoot){
            VerbSequence lastVerbSequence = null;
            foreach(XmlNode childNode in xmlRoot.ChildNodes){
                string childName = childNode.Name.ToLower();
                if(VerbSequence.SA_StringToType.TryGetValue(childName, out Type type)){
                    applyNext(ref lastVerbSequence, type, childNode);
                    //VerbSequence verbSeq = (VerbSequence)VerbSequence.getGenericMethod(type).Invoke(null, BindingFlags.Public | BindingFlags.Static, null, new object[] { childNode, null }, null);
                    //sequence.Add(verbSeq);
                }else{
                    Log.Warning("Matching Type not found " + childName);
                }
            }
		}
    }

    public class VerbPartialScript : Def{
        public VerbScript verbScript;
        public static VerbPartialScript named(string defName){
            return DefDatabase<VerbPartialScript>.GetNamed(defName);
        }
    }

    public class VerbExtraTargetAction{
        public VerbScript allowTarget;
        public VerbScript onCancel;

        public float range = -1;
        public VerbScript cellHighlight;
        public string mouseTexturePath;
        public Texture2D private_mouseTexture;
        public Texture2D MouseTexture{
            get{
                if(private_mouseTexture == null){
                    if(mouseTexturePath == null){
                        private_mouseTexture = TexCommand.Attack;
                    }else{
                        private_mouseTexture = ContentFinder<Texture2D>.Get(mouseTexturePath);
                    }
                }
                return private_mouseTexture;
            }
        }
    }

    public class VerbData {
        public VerbScriptDef def;
        public VerbScript scriptChannel(int i){
            switch(i){
                case 0:{
                    return potential;
                }
                case 1:{
                    return allow;
                }
                case 2:{
                    return allowTarget;
                }
                case 3:{
                    return init;
                }
                case 4:{
                    return fire;
                }
                /**case 4:{
                    return aimTick;
                }**/
                case 5:{
                    return ai_activeTargets;
                }
                case 6:{
                    return ai_targetPoints;
                }
                case 7:{
                    return ai_targetMode;
                }
                case 8:{
                    return cancel;
                }
                case 9:{
                    return cellHighlight;
                }
            }
            return null;
        }
        public const int scriptChannelLength = 10;

        public VerbScript potential;
        public VerbScript allow;
        public VerbScript allowTarget;
        public List<VerbExtraTargetAction> extraTarget;
        public VerbScript cellHighlight;

        public VerbScript init;
        //public VerbScript aimTick;
        public VerbScript fire;
        public VerbScript cancel;

        public bool needsLineOfSight{
            get{
                return verbProps.requireLineOfSight;
            }
        }
        public bool AICanEverUse{
            get{
                return aiEnabled;
            }
        }
        public VerbScript ai_activeTargets;// = new VerbScript(); // -> return all
        public VerbScript ai_targetPoints;// = new VerbScript(); // -> return last
        public VerbScript ai_targetMode;// = new VerbScript(); // -> return last
        public int cooldownTicks;
        public int initialCooldownTicks = 0;

        public bool noTarget = false;
        public bool useDefaultAI = false;
        public bool aiEnabled = false;
        public bool repeatVerb = false;
        public bool isViolent = true;
        public string label;
        public string description;
        public string icon;

        public VerbProperties verbProps;
        public int ID;
        public static int SA_IDC = 0;

        //public List<VerbSequence> animationConditions = new List<VerbSequence>();
        public VerbSequence firstMatchingFilterGroup(VerbSequence groupFirstInstance, int channel) {
            long targetGroup = groupFirstInstance.CT_groupingHash();
            if (targetGroup == -1) {
                foreach (VerbSequence sfit in scriptChannel(channel).sequence) {
                    if(sfit is VerbScope vss){
                        foreach(VerbSequence vsP in vss.skipScopeRightTypeEnumerable()){
                            if(vsP.isIdentitical(groupFirstInstance)) {
                                return vsP;
                            }
                        }
                    }else{
                        if (sfit.isIdentitical(groupFirstInstance)) {
                            return sfit;
                        }
                    }
                }
            } else {
                foreach (VerbSequence sfit in scriptChannel(channel).sequence) {
                    if(sfit is VerbScope vss){
                        foreach(VerbSequence vsP in vss.skipScopeRightTypeEnumerable()){
                            if(vsP.CT_groupingHash() == targetGroup) {
                                return vsP;
                            }
                        }
                    }else{
                        if (sfit.CT_groupingHash() == targetGroup) {
                            return sfit;
                        }
                    }
                }
            }
            Log.Error("Could not find first matching filter! Please report this error. " + groupFirstInstance);
            return null;
        }

        //public static Dictionary<int, VerbSequence> SA_UniqueSituationFilter = new Dictionary<int, VerbSequence>();
        //public static HashSet<int> SA_CollidingHashCheck = new HashSet<int>();

        //-1 is reserved for all, if index doesnt exist
        public static Dictionary<int, List<VerbData>> SA_ThingDefIndexToUncompiled = new Dictionary<int, List<VerbData>>();
        public static List<VerbData> getOrCreateLVD(int index){
            if(!SA_ThingDefIndexToUncompiled.ContainsKey(index)){
                SA_ThingDefIndexToUncompiled.Add(index, new List<VerbData>());
            }
            return SA_ThingDefIndexToUncompiled[index];
        }
        public static Dictionary<int, VerbSequenceRoot> SA_ThingDefIndexToTree = new Dictionary<int, VerbSequenceRoot>();
        public static bool DebugLog = false;
        public static void preEnsureAllUnique(){
            foreach(VerbScriptDef vbs in DefDatabase<VerbScriptDef>.AllDefs){
                VerbData vb = vbs.verbData;
                vb.def = vbs;
                if(vb.noTarget){
                    vb.verbProps.targetParams.canTargetSelf = true;
                }
                if(vb.allow != null){
                    VC_GroupableAON_AND vcAND = new VC_GroupableAON_AND();
                    vcAND.verbSequences.AddRange(vb.allow.sequence);
                    vb.allow.sequence.Clear();
                    vb.allow.sequence.Add(vcAND);
                }
                if(vb.allowTarget != null){
                    VC_GroupableAON_AND vcAND = new VC_GroupableAON_AND();
                    vcAND.verbSequences.AddRange(vb.allowTarget.sequence);
                    vb.allowTarget.sequence.Clear();
                    vb.allowTarget.sequence.Add(vcAND);
                }
                if(vb.aiEnabled){
                    int d3 = 0;
                    if(vb.noTarget){
                        if(vb.ai_activeTargets == null){
                            vb.ai_activeTargets = new VerbScript();
                            vb.ai_activeTargets.sequence.Add(new VS_RootScope());
                        }
                    }else{
                        if(vb.ai_activeTargets == null){
                            d3 += 1;
                            vb.ai_activeTargets = new VerbScript();
                            vb.ai_activeTargets.sequence.Add(new VE_HostilePawnsInMap());
                        }
                    }
                    if(vb.ai_targetMode == null){
                        d3 += 1;
                        vb.ai_targetMode = new VerbScript();
                        vb.ai_targetMode.sequence.Add(new VEC_Fire());
                    }
                    if(vb.ai_targetPoints == null){
                        d3 += 1;
                        vb.ai_targetPoints = new VerbScript();
                        vb.ai_targetPoints.sequence.Add(new VE_Number(){ number = 1.0f });
                    }
                    if(d3 == 3){
                        vb.useDefaultAI = true;
                    }
                }else{
                    if(vb.ai_activeTargets != null){
                        Log.Warning("Field ai_activeTargets exists but aiEnabled is false.");
                    }else{
                        vb.ai_activeTargets = new VerbScript();
                    }
                    if(vb.ai_targetMode != null){
                        Log.Warning("Field ai_targetMode exists but aiEnabled is false.");
                    }else{
                        vb.ai_targetMode = new VerbScript();
                    }
                    if(vb.ai_targetPoints != null){
                        Log.Warning("Field ai_targetPoints exists but aiEnabled is false.");
                    }else{
                        vb.ai_targetPoints = new VerbScript();
                    }
                }

                if(vb.ai_activeTargets != null){
                    VE_JointSequence joSeq = new VE_JointSequence();
                    joSeq.verbSequences.AddRange(vb.ai_activeTargets.sequence);

                    VE_ReturnLast velScore = new VE_ReturnLast();
                    velScore.verbSequences.AddRange(vb.ai_targetPoints.sequence);
                    vb.ai_targetPoints.sequence.Clear();
                    
                    VE_ReturnLast velMode = new VE_ReturnLast();
                    velMode.verbSequences.AddRange(vb.ai_targetMode.sequence);
                    vb.ai_targetMode.sequence.Clear();

                    VE_OPT_A8717E0 a8 = new VE_OPT_A8717E0();
                    a8.ENUMERABLE = joSeq;
                    a8.MODEEVALUATE = velMode;
                    a8.POINTEVALUATE = velScore;
                    a8.RANGE = vbs.verbData.verbProps.range;
                    
                    if(vb.allowTarget != null){
                        VE_FilterSequence VSSFS = new VE_FilterSequence();
                        VSSFS.verbSequences.Add(joSeq);
                        VSSFS.filterCondition = vb.allowTarget.sequence[0];
                        a8.ENUMERABLE = VSSFS;
                    }
                    vb.ai_targetPoints.sequence.Add(a8);
                }
            }
        }
        public static void ensureAllUnique(){
            int iterationNum = 0;
            List<VerbSequence> verbSequences = new List<VerbSequence>();
            List<VerbScope> verbScopesDefault = new List<VerbScope>();
            for(int j = 0; j < scriptChannelLength; j++){
                foreach(VerbScriptDef vbs in DefDatabase<VerbScriptDef>.AllDefs){
                    VerbData vb = vbs.verbData;
                    if(j == 0){
                        vb.cancel?.setName(vbs.defName + "_BE1AAF0F_CANCEL");
                        vb.fire?.setName(vbs.defName + "_BE1AAF0F_FIRE");
                        vb.init?.setName(vbs.defName + "_BE1AAF0F_INIT");
                        vb.allow?.setName(vbs.defName + "_BE1AAF0F_ALLOW");
                        vb.allowTarget?.setName(vbs.defName + "_BE1AAF0F_ALLOWTARGET");
                        vb.potential.setName(vbs.defName + "_BE1AAF0F_POTEN");
                        vb.ai_targetPoints.setName(vbs.defName + "_BE1AAF0F_POINTS");
                        vb.ai_activeTargets.setName(vbs.defName + "_BE1AAF0F_ACTIVETARGETS");
                        vb.ai_targetMode.setName(vbs.defName + "_BE1AAF0F_TARGETMODE");
                        vb.cellHighlight?.setName(vbs.defName + "_BE1AAF0F_CELLHIGHLIGHT");
                    }

                    vb.ID = SA_IDC;
                    SA_IDC++;
                    VerbScript vs = vb.scriptChannel(j);
                    if(vs != null){
                        func9928371(vs, j == 0, verbScopesDefault);
                        if(j == 0 && vs.sequence.Count != 0){
                            GroupStateSet gss = GroupStateUtility.compareSet<VC_ThingDef>(vs.sequence);
                            if(gss.defaultState == -1){
                                foreach(KeyValuePair<int, int> ii in gss.subIndexToState){
                                    getOrCreateLVD(ii.Key).Add(vb);
                                    if(ii.Value != 1){
                                        Log.Error("Unexpected value in GSS subIndexToState");
                                    }
                                }
                                if(gss.subIndexToState.Count == 0){
                                    Log.Error("No subIndexes!");
                                }
                            }else{
                                getOrCreateLVD(-1).Add(vb);
                            }
                        }
                    }
                }
                //after 0 and 1 do this to create relation set
                if(j == 0){
                    foreach(VerbSequence verbSeq in VerbSequence.verbEffectUniqueInstance.Values){
                        if(verbSeq is VerbScope vss){
                            if(vss.scopeLeftType() != ScopeLeftType.Root){
                                verbSequences.AddRange(vss.skipScopeRightTypeEnumerable());
                            }
                        }else{
                            verbSequences.Add(verbSeq);
                        }
                    }
                }
            }
            
            foreach(VerbScriptDef vbs in DefDatabase<VerbScriptDef>.AllDefs){
                VerbData vb = vbs.verbData;
                if(vb.extraTarget != null){
                    for(int i = 0; i < vb.extraTarget.Count; i++){
                        if(vb.extraTarget[i].cellHighlight == null){
                            if(vb.extraTarget[i].range == -1){
                                vb.extraTarget[i].range = vb.verbProps.range;
                            }
                            VE_CellsInRadius vet = new VE_CellsInRadius();
                            vet.position = new VS_RootScope();
                            vet.radius = new VE_Number(){ number = vb.extraTarget[i].range };
                            vet.requiresLineOfSight = vb.verbProps.requireLineOfSight;
                            vb.extraTarget[i].cellHighlight = new VerbScript();
                            vb.extraTarget[i].cellHighlight.sequence.Add(vet);
                        }

                        vb.extraTarget[i].allowTarget.setName(vbs.defName + "_BE1AAF0F_EXTRATARGET_" + i + "_ALLOWTARGET");
                        vb.extraTarget[i].onCancel?.setName(vbs.defName + "_BE1AAF0F_EXTRATARGET_" + i + "_ONCANCEL");
                        vb.extraTarget[i].cellHighlight.setName(vbs.defName + "_BE1AAF0F_EXTRATARGET_" + i + "_VALIDCELLS");

                        VC_GroupableAON_AND vcAND = new VC_GroupableAON_AND();
                        vcAND.verbSequences.AddRange(vb.extraTarget[i].allowTarget.sequence);
                        vb.extraTarget[i].allowTarget.sequence.Clear();
                        vb.extraTarget[i].allowTarget.sequence.Add(vcAND);

                        func9928371(vb.extraTarget[i].allowTarget, false, verbScopesDefault);
                        if(vb.extraTarget[i].onCancel != null){
                            func9928371(vb.extraTarget[i].onCancel, false, verbScopesDefault);
                        }
                    }
                }
            }
            foreach(ThingDef tds in DefDatabase<ThingDef>.AllDefs){
                CompProperties_ScriptExecutor cp = tds.GetCompProperties<CompProperties_ScriptExecutor>();
                if(cp != null){
                    func9928371(cp.verbScript, true, verbScopesDefault);
                }
            }

            //Log.Warning("AllElementNum " + iterationNum);
            //Log.Warning("UniqueElementNum " + verbSequences.Count);

            HashSet<VerbSequence> veTopStack = VerbSequence.topLayer(verbSequences);
        }

        public static void func9928371(VerbScript vss, bool canClearAND, List<VerbScope> verbScopesDefault){
            for(int i = 0; i < vss.sequence.Count; i++) {
                VerbSequence sfilter = vss.sequence[i];
                if(canClearAND){
                    if(sfilter is VC_GroupableAON_AND asi){
                        vss.sequence.AddRange(asi.verbSequences);
                        vss.sequence.RemoveAt(i);
                        i--;
                        continue;
                    } 
                }
                vss.sequence[i] = sfilter.registerAllSubVerbSequencesAndReturn(verbScopesDefault, ScopeLeftType.Root);
                if(sfilter != vss.sequence[i]){
                    //Log.Message("Merged " + sfilter + " into " + vb.scriptChannel(j).sequence[i]);
                }
            }
        }

        public static VerbSequenceRoot initializeAll(List<VerbData> aolp, int channel){
            VerbSequenceRoot rootTree = VerbSequenceRoot.nextACT();
            Dictionary<VerbData, HashSet<VerbSequence>> sParam = new Dictionary<VerbData, HashSet<VerbSequence>>();
            foreach(VerbData aol in aolp){
                HashSet<VerbSequence> hs = new HashSet<VerbSequence>();
                sParam.Add(aol, hs);
                foreach(VerbSequence verbSeq in aol.scriptChannel(channel).sequence){
                    if(verbSeq is VerbScope vss){
                        hs.AddRange(vss.skipScopeRightTypeEnumerable());
                    }else{
                        hs.Add(verbSeq);
                    }
                }
                //hs.AddRange(aol.scriptChannel(channel).sequence);
            }

            rootTree.trySort(sParam, channel);
            //rootTree.logTest();
            return rootTree;
        }


    }


}
