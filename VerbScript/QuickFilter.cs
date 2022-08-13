using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbScript {
    public class VerbQuickFilter {
        public static VerbQuickFilter SAQFI_Job = new VQF_Job();
        public static VerbQuickFilter SAQFI_Draft = new VQF_Draft();
        public static VerbQuickFilter SAQFI_Moving = new VQF_Moving();
        public static VerbQuickFilter SAQFI_Hediff = new VQF_Hediff();
        public static VerbQuickFilter SAQFI_Trait = new VQF_Trait();
        public static VerbQuickFilter SAQFI_Equipment = new VQF_Equipment();
        public static VerbQuickFilter SAQFI_MentalState = new VQF_MentalState();
        public virtual bool needsRefresh(VerbRootQD QD, int subIndex = 0){
            for(int i = 0; i < multiTypeMatch_Indexed.Count; i++){
                HashSet<int> hashInt = QD.func2394(multiTypeMatch_Indexed[i]);
                if(hashInt != null && hashInt.Contains(subIndex)){
                    return true;
                }
            }
            for(int i = 0; i < multiTypeMatch_NonIndexed.Count; i++){
                HashSet<int> hashInt = QD.func2394(multiTypeMatch_NonIndexed[i]);
                if(hashInt != null && hashInt.Contains(0)){
                    return true;
                }
            }
            return false;
        }
        public List<Type> multiTypeMatch_Indexed = new List<Type>();
        public List<Type> multiTypeMatch_NonIndexed = new List<Type>();

        public static Dictionary<Type, List<VerbQuickFilter>> SA_TypeToFilters = new Dictionary<Type, List<VerbQuickFilter>>();
    }
    public class VQF_Trait : VerbQuickFilter {
        public VQF_Trait() : base(){
            multiTypeMatch_Indexed.Add(typeof(VC_HasTrait));
        }
    }
    public class VQF_Hediff : VerbQuickFilter {
        public VQF_Hediff() : base(){
            multiTypeMatch_Indexed.Add(typeof(VC_HasHediff));
        }
    }
    public class VQF_Equipment : VerbQuickFilter {
        public VQF_Equipment() : base(){
            multiTypeMatch_Indexed.Add(typeof(VC_HasEquipment));
        }
    }
    public class VQF_Job : VerbQuickFilter {
        public VQF_Job() : base(){
            multiTypeMatch_Indexed.Add(typeof(VC_CurJobDef));
        }
    }
    public class VQF_Draft : VerbQuickFilter {
        public VQF_Draft() : base(){
            multiTypeMatch_Indexed.Add(typeof(VC_Drafted));
        }
    }
    public class VQF_Moving : VerbQuickFilter {
        public VQF_Moving() : base(){
            multiTypeMatch_Indexed.Add(typeof(VC_Moving));
        }
    }
    public class VQF_MentalState : VerbQuickFilter {
        public VQF_MentalState() : base(){
            multiTypeMatch_Indexed.Add(typeof(VC_InMentalState));
        }
    }

    public class VerbStateTracker{
        public HashSet<VerbQuickFilter> AQF = new HashSet<VerbQuickFilter>();
        public void RegisterState(VerbQuickFilter aqf){
            AQF.Add(aqf);
        }
        public bool needsRefresh(VerbRootQD anqd, bool clearAfter = false) {
            foreach(VerbQuickFilter aq in AQF){
                if(aq.needsRefresh(anqd)){
                    if(clearAfter){ clear(); }
                    return true;
                }
            }
            if(clearAfter){ clear(); }
            return false;
        }
        public void clear(){
            AQF.Clear();
        }
    }

    public class VerbRootQD{
        public static List<HashSet<int>> SA_FreeHashInt = new List<HashSet<int>>();
        public static HashSet<int> nextHashSet(){
            if(SA_FreeHashInt.Count > 0){
                return SA_FreeHashInt.Pop();
            }
            return new HashSet<int>();
        }

        public Dictionary<Type, HashSet<int>> indexedHook = new Dictionary<Type, HashSet<int>>();
        //public HashSet<Type> nonIndexedHook = new HashSet<Type>();
        public void logTest(){
            Log.Warning("-------Condition Listeners-------");
            foreach(Type t in indexedHook.Keys){
                Log.Warning(t.Name);
            }
        }

        public void clear() {
            //Log.Warning("Imbeing cleaered " + Rand.Value);
            foreach(HashSet<int> hsi in indexedHook.Values){
                hsi.Clear();
                SA_FreeHashInt.Add(hsi);
            }
            indexedHook.Clear();
        }
        public HashSet<int> func2394(Type type, bool generateIfNotExist = false){
            if(!indexedHook.ContainsKey(type)){
                if(generateIfNotExist){
                    indexedHook.Add(type, nextHashSet());
                }else{
                    return null;
                }
            }
            return indexedHook[type];
        }
        public bool hasType(Type T){
            return indexedHook.ContainsKey(T);
        }

    }

}
