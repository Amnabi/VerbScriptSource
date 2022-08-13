using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace VerbScript {

    public class VerbSequenceNode{
        public virtual void free(){
        }
        
        public static string tabs(int s){
            return s == 0 ? "" : tabs(s - 1) + "  ";
        }
        public static StringBuilder SA_LogStringBuilder = new StringBuilder();
        public void logTest(){
            SA_LogStringBuilder.Clear();
            func1979736();
            string outt = SA_LogStringBuilder.ToString();
            //Log.Warning("Length " + outt.Length);
            Log.Warning("HASHCODE " + outt.GetHashCode());
            Log.Warning(outt);
        }
        public void func1979736(int depth = 0){
            if(this is VerbSequenceRoot acr){
                //acr.quickData.logTest();
            }
            foreach(VerbData lsd in currentVerbDatas){
                SA_LogStringBuilder.AppendLine(tabs(depth) + "FilterContent " + lsd.label);
                //Log.Warning(tabs(depth) + "FilterContent " + lsd + lsd.GetHashCode());
            }
            foreach(KeyValuePair<int, VerbSequenceNode> kvp in subTreeSwitch){
                SA_LogStringBuilder.AppendLine(tabs(depth) + "[" + selfFilter.GetType().Name + " SubIndex " + kvp.Key + "]" + selfFilter.uniqueID.text);
                //Log.Warning(tabs(depth) + "[" + selfFilter.GetType().Name + " SubIndex " + kvp.Key + "]" + selfFilter.uniqueID.text);
                kvp.Value.func1979736(depth + 1);
            }

        }

        //public static VerbSequenceBranch rootTree;

        public VerbSequence selfFilter;
        public List<VerbData> currentVerbDatas = new List<VerbData>();
        public Dictionary<int, VerbSequenceNode> subTreeSwitch = new Dictionary<int, VerbSequenceNode>();//-1 should always be irrelevant filter
        public IEnumerable<VerbData> allMatch(Pawn p){
            VerbSequence.ClearCache();
            SA_IterationStack.Clear();
            //Log.Warning("START " + p);
            allValidVerbDatas(SA_IterationStack, p);
            /**if(SA_IterationStack.Count == 0){
                return SA_IterationStack;
            }**/
            return SA_IterationStack;
        }
        public void allValidVerbDatas(List<VerbData> validDefStack, Pawn p){
            //Log.Warning("What " + currentVerbDatas.Count);
            //currentVerbDatas.ForEach(x => Log.Warning("Adding " + x.label));
                
            validDefStack.AddRange(currentVerbDatas);
            if(selfFilter != null){
                ExecuteStackContext.SA_StaticContext.clear();
                ExecuteStackContext.SA_StaticContext.rootScope = p;
                IEnumerable<int> ii = ExecuteStackContext.SA_StaticContext.batchScopeExecute(selfFilter.leftHandParentScopes, selfFilter);//selfFilter.stateEvaluate(ExecuteStackContext.SA_StaticContext);

                //Log.Warning(i + " " + selfFilter.uniqueID);
                bool min1Done = false;
                VerbSequenceNode tryOut;
                foreach(int i in ii){
                    //Log.Warning("Checking for " + p + " / " + i);
                    if(i == -1){
                        min1Done = true;
                    }
                    if(subTreeSwitch.TryGetValue(i, out tryOut)){
                        tryOut.allValidVerbDatas(validDefStack, p);
                    }
                }
                if(!min1Done && subTreeSwitch.TryGetValue(-1, out tryOut)){
                    tryOut.allValidVerbDatas(validDefStack, p);
                }
            }
        }

        public virtual void trySort(Dictionary<VerbData, HashSet<VerbSequence>> keyToValueRemaining, int channel){
            //filter out defs with no remaining
            foreach(KeyValuePair<VerbData, HashSet<VerbSequence>> sitDef in keyToValueRemaining){
                if(sitDef.Value.Count == 0){
                    currentVerbDatas.Add(sitDef.Key);
                }
            }
            foreach(VerbData rFilt in currentVerbDatas){
                keyToValueRemaining.Remove(rFilt);
            }

            if(keyToValueRemaining.Count == 0){
                //if(VerbData.DebugLog){
                    //Log.Message("End Node - 0 Count");
                //}
                return;
            }

            //PICK BEST
            //default/irrelavant + lowest case
            clearStatic();

            List<VerbSequence> verbSequencesThisAll = new List<VerbSequence>();
            //Sort them into groups by CT_groupingHash
            foreach(KeyValuePair<VerbData, HashSet<VerbSequence>> sitDef in keyToValueRemaining){
                VerbData sdKey = sitDef.Key;
                foreach(VerbSequence sitFilt in sitDef.Value){
                    long CT_groupingHash = sitFilt.CT_groupingHash();
                    if(CT_groupingHash == -1){
                        CT_groupingHash = nextDynaID();
                    }
                    if(!SA_GrouppedFilters.ContainsKey(CT_groupingHash)){
                        SA_GrouppedFilters.Add(CT_groupingHash, nextLSF());
                        SA_GrouppedFiltersReverse.Add(CT_groupingHash, nextHashSetVerbData());
                    }
                    SA_GrouppedFilters[CT_groupingHash].Add(sitFilt);
                    SA_GrouppedFiltersReverse[CT_groupingHash].Add(sdKey);
                }
                verbSequencesThisAll.AddRange(sitDef.Value);
            }
            //rank them by points
            foreach(KeyValuePair<long, List<VerbSequence>> kisfv in SA_GrouppedFilters){
                SA_ScoreSubEval.Clear();
                long groupH = kisfv.Key;
                List<VerbSequence> filters = kisfv.Value;
                HashSet<VerbData> defsToEval = SA_GrouppedFiltersReverse[groupH];
                VerbSequence firstInstance = filters[0];
                int remainderIrrelevant = keyToValueRemaining.Count - defsToEval.Count;
                foreach(VerbData defCheck in defsToEval){
                    //MAY NEED PATCH, onlt returns 1
                    long outputState = defCheck.firstMatchingFilterGroup(firstInstance, channel).uniqueSubIDFromContent();//firstInstance.UT_outputState(defCheck, channel);
                    if(!SA_ScoreSubEval.ContainsKey(outputState)){
                        SA_ScoreSubEval.Add(outputState, 1);
                    }else{
                        int prev = SA_ScoreSubEval[outputState];
                        SA_ScoreSubEval.Remove(outputState);
                        SA_ScoreSubEval.Add(outputState, prev + 1);
                    }
                }
                int maxValue = -1;
                foreach(int ok in SA_ScoreSubEval.Values){
                    if(maxValue == -1 || maxValue <= ok){
                        maxValue = ok;
                    }
                }
                maxValue += remainderIrrelevant;
                SA_ScoreEval.Add(groupH, maxValue);
            }
            //and select lowest group
            long bestHash = -1;
            float lowestVal = -1;
            float vN;
            foreach(KeyValuePair<long, float> kvp in SA_ScoreEval){
                vN = kvp.Value;
                if(lowestVal == -1 || lowestVal >= vN){
                    bestHash = kvp.Key;
                    lowestVal = vN;
                }
            }
            if(bestHash == -1){
                Log.Error("Failed to retrieve lowest value!");
            }

            //Best hash redirection Check
            //Single Type, redirection benefits
            if(SA_GrouppedFilters[bestHash].Count == 1){
                HashSet<VerbSequence> verbEffTopLayer = VerbSequence.topLayerTemp(verbSequencesThisAll);
                
                //redirect
                bestHash = -1;
                lowestVal = -1;
                foreach(VerbSequence kvp in verbEffTopLayer){
                    long gHL = kvp.CT_groupingHash();
                    vN = SA_ScoreEval[gHL];
                    if(lowestVal == -1 || lowestVal >= vN){
                        bestHash = gHL;
                        lowestVal = vN;
                    }
                }
                if(bestHash == -1){
                    Log.Error("Redirection : Failed to retrieve lowest value!");
                }

            }else{
                //Dont redirect best hash
            }

            //all verbeffects that belong to the best group
            //sort them by sub ID output
            List<VerbSequence> bestTypeChildDetermine = SA_GrouppedFilters[bestHash];
            selfFilter = SA_GrouppedFilters[bestHash][0];
            Dictionary<int, List<VerbData>> subIDToFilters = new Dictionary<int, List<VerbData>>();
            foreach(VerbData affectedDefs in SA_GrouppedFiltersReverse[bestHash]){
                HashSet<VerbSequence> veHS = keyToValueRemaining[affectedDefs];
                VerbSequence outF = filterOutFromHashSet(veHS, selfFilter);
                int subID = outF.uniqueSubIDFromContent();
                if(!subIDToFilters.ContainsKey(subID)){
                    subIDToFilters.Add(subID, new List<VerbData>());
                }
                subIDToFilters[subID].Add(affectedDefs);
            }

            //Warning! O(n^2 * log(n))
            HashSet<VerbData> inclu = SA_GrouppedFiltersReverse[bestHash];
            HashSet<VerbData> childShiftFilter = new HashSet<VerbData>();
            foreach(KeyValuePair<VerbData, HashSet<VerbSequence>> sitDef in keyToValueRemaining){
                if(!inclu.Contains(sitDef.Key)){
                    int index = -1;
                    foreach(VerbSequence vEffectChildCheck in sitDef.Value){//dup match? nope
                        for(int i = 0; i < bestTypeChildDetermine.Count; i++){
                            if(bestTypeChildDetermine[i].childSetTotal(true).Contains(vEffectChildCheck)){
                                if(index != -1){
                                    Log.Error("Duplicate categorization for predessessor node");
                                }
                                index = i;
                                //Log.Warning("AZ " + vEffectChildCheck.uniqueID.text);
                            }
                        }
                    }
                    if(index != -1){
                        int subID = bestTypeChildDetermine[index].uniqueSubIDFromContent();
                        if(!subIDToFilters.ContainsKey(subID)){
                            subIDToFilters.Add(subID, new List<VerbData>());
                        }
                        subIDToFilters[subID].Add(sitDef.Key);
                        childShiftFilter.Add(sitDef.Key);
                    }
                }
            }

            Dictionary<int, Dictionary<VerbData, HashSet<VerbSequence>>> nextStack = new Dictionary<int, Dictionary<VerbData, HashSet<VerbSequence>>>();
            foreach(KeyValuePair<int, List<VerbData>> kvp in subIDToFilters){
                if(kvp.Key == -1){
                    Log.Error("Key -1 is reserved for irrelevant filter!");
                }
                Dictionary<VerbData, HashSet<VerbSequence>> nextFilter = new Dictionary<VerbData, HashSet<VerbSequence>>();
                foreach(VerbData sDef in kvp.Value){
                    nextFilter.Add(sDef, keyToValueRemaining[sDef]);
                }
                nextStack.Add(kvp.Key, nextFilter);
            }
            
            //-1
            Dictionary<VerbData, HashSet<VerbSequence>> nextFilter2 = new Dictionary<VerbData, HashSet<VerbSequence>>();
            foreach(KeyValuePair<VerbData, HashSet<VerbSequence>> sitDef in keyToValueRemaining){
                if(!inclu.Contains(sitDef.Key) && !childShiftFilter.Contains(sitDef.Key)){
                    nextFilter2.Add(sitDef.Key, sitDef.Value);
                }
            }
            if(nextFilter2.Count > 0){
                nextStack.Add(-1, nextFilter2);
            }

            foreach(KeyValuePair<int, Dictionary<VerbData, HashSet<VerbSequence>>> kvp in nextStack){
                //VerbSequenceBranch subNew = new VerbSequenceBranch();
                VerbSequenceBranch subNew = new VerbSequenceBranch();
                subTreeSwitch.Add(kvp.Key, subNew);
                subNew.trySort(kvp.Value, channel);
            }
        }
        
        public static VerbSequence filterOutFromHashSet(HashSet<VerbSequence> sitDef, VerbSequence filterGroup){
            bool noGroup = filterGroup.CT_groupingHash() == -1;
            VerbSequence sitFilt = noGroup ? sitDef.First(x => x.isIdentitical(filterGroup)) : sitDef.First(x => x.isSameGroup(filterGroup));
            //int count = noGroup? sitDef.RemoveWhere(x => x.isIdentitical(filterGroup)) : sitDef.RemoveWhere(x => x.isSameGroup(filterGroup));
            sitDef.Remove(sitFilt);
            //if(count != 1){
            //    Log.Error("Unexpected number of elements removed " + count);
            //}
            //if((noGroup ? sitDef.FirstOrDefault(x => x.isIdentitical(filterGroup)) : sitDef.FirstOrDefault(x => x.isSameGroup(filterGroup))) != null){
            //    Log.Warning("---------------DUAL GROUP DECTECTED");
            //}

            return sitFilt;
        }

        public const long dynamicallyGeneratedIDMax = int.MaxValue / 2;//20000;
        public static long SA_dynamicallyGeneratedID = -1;
        public static List<VerbData> SA_IterationStack = new List<VerbData>();
        //public static HashSet<VerbSequence> SA_MergedFilters = new HashSet<VerbSequence>();//do i need this?
        public static Dictionary<long, HashSet<VerbData>> SA_GrouppedFiltersReverse = new Dictionary<long, HashSet<VerbData>>();
        public static Dictionary<long, List<VerbSequence>> SA_GrouppedFilters = new Dictionary<long, List<VerbSequence>>();
        public static Dictionary<long, float> SA_ScoreEval = new Dictionary<long, float>();
        public static Dictionary<long, int> SA_ScoreSubEval = new Dictionary<long, int>();
        public static void clearStatic(){
            //SA_MergedFilters.Clear();
            SA_ScoreEval.Clear();
            SA_ScoreSubEval.Clear();
            foreach(HashSet<VerbData> sf in SA_GrouppedFiltersReverse.Values){
                free(sf);
            }
            SA_GrouppedFiltersReverse.Clear();
            foreach(List<VerbSequence> sf in SA_GrouppedFilters.Values){
                free(sf);
            }
            SA_GrouppedFilters.Clear();
            SA_dynamicallyGeneratedID = -1;
        }
        public static long nextDynaID(){
            SA_dynamicallyGeneratedID += 1;
            return SA_dynamicallyGeneratedID;
        }

        public static List<List<VerbSequence>> free_VerbSequenceLists = new List<List<VerbSequence>>();
        public static List<HashSet<VerbData>> free_VerbSequenceReverseSets = new List<HashSet<VerbData>>();
        public static void free(List<VerbSequence> lsf){
            lsf.Clear();
            free_VerbSequenceLists.Add(lsf);
        }
        public static void free(HashSet<VerbData> hssfr){
            hssfr.Clear();
            free_VerbSequenceReverseSets.Add(hssfr);
        }
        public static List<VerbSequence> nextLSF(){
            if(free_VerbSequenceLists.Count > 0){
                return free_VerbSequenceLists.Pop();
            }
            return new List<VerbSequence>();
        }
        public static HashSet<VerbData> nextHashSetVerbData(){
            if(free_VerbSequenceReverseSets.Count > 0){
                return free_VerbSequenceReverseSets.Pop();
            }
            return new HashSet<VerbData>();
        }
    }
    public class VerbSequenceRoot : VerbSequenceNode{
        public static List<VerbSequenceRoot> SA_freeACR = new List<VerbSequenceRoot>();
        public VerbRootQD quickData = new VerbRootQD();
        public static VerbSequenceRoot nextACT(){
            if(SA_freeACR.Count > 0){
                return SA_freeACR.Pop();
            }
            return new VerbSequenceRoot();
        }
        public override void free(){
            selfFilter = null;
            //currentVerbDatas.ForEach(x => x.free());
            currentVerbDatas.Clear();
            foreach(VerbSequenceNode act in subTreeSwitch.Values){
                act.free();
            }
            quickData.clear();
            subTreeSwitch.Clear();
            SA_freeACR.Add(this);
        }
        //public static HashSet<VerbSequence> SA_ACSort = new HashSet<VerbSequence>();
        public override void trySort(Dictionary<VerbData, HashSet<VerbSequence>> keyToValueRemaining, int channel) {
            /**SA_ACSort.Clear();
            foreach(HashSet<VerbSequence> animCond in keyToValueRemaining.Values){
                SA_ACSort.AddRange(animCond);
            }
            foreach(VerbSequence ac in SA_ACSort){
                quickData.func2394(ac.GetType(), true).Add(ac.uniqueSubIDFromContent());
            }**/
            foreach(HashSet<VerbSequence> animCond in keyToValueRemaining.Values){
                foreach(VerbSequence ve in animCond){
                    ve.RegisterAllTypes(quickData);
                }
            }
            base.trySort(keyToValueRemaining, channel);
        }

    }

    public class VerbSequenceBranch : VerbSequenceNode{
        public static List<VerbSequenceBranch> SA_freeACT = new List<VerbSequenceBranch>();
        public static VerbSequenceBranch nextACT(){
            if(SA_freeACT.Count > 0){
                return SA_freeACT.Pop();
            }
            return new VerbSequenceBranch();
        }
        public override void free(){
            selfFilter = null;
            //currentVerbDatas.ForEach(x => x.free());
            currentVerbDatas.Clear();
            foreach(VerbSequenceBranch act in subTreeSwitch.Values){
                act.free();
            }
            subTreeSwitch.Clear();
            SA_freeACT.Add(this);
        }
    }
}
