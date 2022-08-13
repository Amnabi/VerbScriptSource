using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace BFPTN {

    public class ThingDefKeyExtendedData{
        public ThingDef thingDefKey;
        public int count;
        public void clear(){
            thingDefKey = null;
            count = 0;
        }

        public static List<ThingDefKeyExtendedData> freeThingDefKeyExtendedData = new List<ThingDefKeyExtendedData>();
        public ThingDefKeyExtendedData(ThingDef td){
            thingDefKey = td;
        }
        public void free(){
            freeThingDefKeyExtendedData.Add(this);
        }
        public static ThingDefKeyExtendedData nextTDKED(ThingDef td){
            if(freeThingDefKeyExtendedData.Count > 0){
                ThingDefKeyExtendedData rdk = freeThingDefKeyExtendedData.Pop();
                rdk.clear();
                return rdk;
            }
            return new ThingDefKeyExtendedData(td);
        }

        
        public static List<HashSet<BFPTNDef>> freeHashSetBFPTNDef = new List<HashSet<BFPTNDef>>();
        public static void freeHashSet(HashSet<BFPTNDef> hs){
            hs.Clear();
            freeHashSetBFPTNDef.Add(hs);
        }
        public static HashSet<BFPTNDef> nextHashSet(){
            if(freeHashSetBFPTNDef.Count > 0){
                HashSet<BFPTNDef> rdk = freeHashSetBFPTNDef.Pop();
                return rdk;
            }
            return new HashSet<BFPTNDef>();
        }
    }
    public class TraitAndDegreeKeyExtendedData{
        public TraitAndDegree traitDefKey;
        public int count;
        public void clear(){
            traitDefKey = default(TraitAndDegree);
            count = 0;
        }

        public static List<TraitAndDegreeKeyExtendedData> freeThingDefKeyExtendedData = new List<TraitAndDegreeKeyExtendedData>();
        public TraitAndDegreeKeyExtendedData(TraitAndDegree td){
            traitDefKey = td;
        }
        public void free(){
            freeThingDefKeyExtendedData.Add(this);
        }
        public static TraitAndDegreeKeyExtendedData nextTDKED(TraitAndDegree td){
            if(freeThingDefKeyExtendedData.Count > 0){
                TraitAndDegreeKeyExtendedData rdk = freeThingDefKeyExtendedData.Pop();
                rdk.clear();
                return rdk;
            }
            return new TraitAndDegreeKeyExtendedData(td);
        }
    }

    public class MapComponent_BFPTN : MapComponent {
        public void tickSwitch(){
            //HashSet<BFPTNDef> hh = lastActiveBFPTN;
            //lastActiveBFPTN = activeBFPTN;
            //activeBFPTN = hh;

            //Dictionary<ThingDef, ThingDefKeyExtendedData> hh2 = lastThingDefData;
            //lastThingDefData = activeThingDefData;
            //activeThingDefData = hh2;
        }
        //switch these two;
        public override void ExposeData(){
            base.ExposeData();
            //Scribe_Collections.Look<Pawn>(ref lastAffectedPawns, "lastAffectedPawns", LookMode.Reference);
        }
        //use timed hediff instead
        //public List<Pawn> lastAffectedPawns = new List<Pawn>();
        //public HashSet<Pawn> affectedPawnsThisTick = new HashSet<Pawn>();
        //public HashSet<Pawn> lastAffectedPawns_TempHashSet = new HashSet<Pawn>();
        //public HashSet<BFPTNDef> lastActiveBFPTN = new HashSet<BFPTNDef>();
        //public HashSet<BFPTNDef> activeBFPTN = new HashSet<BFPTNDef>();
        public HashSet<BFPTNDef> activeCheckBFPTN = new HashSet<BFPTNDef>();
        public HashSet<BFPTNDef> allTargetingBFPTN = new HashSet<BFPTNDef>();
        public Dictionary<int, BFPTNDef> highestID = new Dictionary<int, BFPTNDef>();
        public Dictionary<ThingDef, HashSet<BFPTNDef>> targetActiveThingDefData = new Dictionary<ThingDef, HashSet<BFPTNDef>>();
        public HashSet<BFPTNDef> targetHashSet(ThingDef td, bool generateIfNotExist){
            if(!targetActiveThingDefData.ContainsKey(td)){
                if(generateIfNotExist){
                    targetActiveThingDefData.Add(td, ThingDefKeyExtendedData.nextHashSet());
                }else{
                    return null;
                }
            }
            return targetActiveThingDefData[td];
        }
        public Dictionary<ThingDef, ThingDefKeyExtendedData> requiredActiveThingDefData = new Dictionary<ThingDef, ThingDefKeyExtendedData>();
        public Dictionary<TraitAndDegree, TraitAndDegreeKeyExtendedData> requiredActiveTraitAndDegreeData = new Dictionary<TraitAndDegree, TraitAndDegreeKeyExtendedData>();
        public ThingDefKeyExtendedData tryGetData(ThingDef key, bool generateIfNotExist){
            if(!requiredActiveThingDefData.ContainsKey(key)){
                if(generateIfNotExist){
                    requiredActiveThingDefData.Add(key, ThingDefKeyExtendedData.nextTDKED(key));
                }else{
                    return null;
                }
            }
            return requiredActiveThingDefData[key];
        }
        public TraitAndDegreeKeyExtendedData tryGetData(TraitAndDegree key, bool generateIfNotExist){
            if(!requiredActiveTraitAndDegreeData.ContainsKey(key)){
                if(generateIfNotExist){
                    requiredActiveTraitAndDegreeData.Add(key, TraitAndDegreeKeyExtendedData.nextTDKED(key));
                }else{
                    return null;
                }
            }
            return requiredActiveTraitAndDegreeData[key];
        }
        public MapComponent_BFPTN(Map map) : base(map) {
        }
        public override void FinalizeInit() {
            base.FinalizeInit();
        }
        public override void MapComponentOnGUI() {
            base.MapComponentOnGUI();
        }
        public override void MapComponentTick() {
            base.MapComponentTick();
            if(Find.TickManager.TicksGame % BFPTNSettings.ticksPerCheck == 0){
                activeCheckBFPTN.Clear();
                //activeBFPTN.Clear();
                foreach(ThingDefKeyExtendedData po in requiredActiveThingDefData.Values){
                    po.free();
                }
                foreach(TraitAndDegreeKeyExtendedData po in requiredActiveTraitAndDegreeData.Values){
                    po.free();
                }
                requiredActiveThingDefData.Clear();
                requiredActiveTraitAndDegreeData.Clear();
                foreach(HashSet<BFPTNDef> hs in targetActiveThingDefData.Values){
                    ThingDefKeyExtendedData.freeHashSet(hs);
                }
                targetActiveThingDefData.Clear();
                allTargetingBFPTN.Clear();
                highestID.Clear();
                //requirement check
                foreach (Pawn pawn in map.mapPawns.PawnsInFaction(Faction.OfPlayer)) {
                    ThingDef pawnDef = pawn.def;
                    HashSet<BFPTNDef> outV;
                    if(BFPTNDef.SA_HS_RequiredThingDefToBFPTN.TryGetValue(pawnDef, out outV)){
                        activeCheckBFPTN.AddRange(outV);
                        tryGetData(pawnDef, true).count += 1;
                    }
                    if(pawn.story != null && pawn.story.traits != null){
                        foreach(Trait tr in pawn.story.traits.allTraits){
                            TraitAndDegree tad = new TraitAndDegree();
                            tad.traitDef = tr.def;
                            tad.degree = tr.Degree;
                            HashSet<BFPTNDef> outV2;
                            if(BFPTNDef.SA_HS_RequiredTraitAndDegreeToBFPTN.TryGetValue(tad, out outV2)){
                                activeCheckBFPTN.AddRange(outV2);
                                tryGetData(tad, true).count += 1;
                            }
                        }
                    }
                }
                foreach(BFPTNDef td in BFPTNDef.SA_HS_CheckRegardless){
                    activeCheckBFPTN.Add(td);
                }
                foreach(BFPTNDef kvp in activeCheckBFPTN){
                    if(kvp.activeCheck(this)){
                        BFPTNDef outVal = null;
                        if(highestID.TryGetValue(kvp.exclusiveTag, out outVal) && outVal.priority < kvp.priority){
                            highestID.Remove(kvp.exclusiveTag);
                            highestID.Add(kvp.exclusiveTag, kvp);
                        }else{
                            highestID.Add(kvp.exclusiveTag, kvp);
                        }
                    }
                }
                foreach(BFPTNDef dftdef in highestID.Values){
                    foreach(ThingDef targetPawn in dftdef.targetPawnDefs){
                        targetHashSet(targetPawn, true).Add(dftdef);
                    }
                    if(dftdef.applyToAllInstead){
                        allTargetingBFPTN.Add(dftdef);
                    }
                }
                /**foreach(BFPTNDef bfp in activeBFPTN){
                    bfp.calculatePower(this);
                }**/
                //affectedPawnsThisTick.Clear();
                //target check
                foreach (Pawn pawn in map.mapPawns.PawnsInFaction(Faction.OfPlayer)){
                    ThingDef pawnDef = pawn.def;
                    HashSet<BFPTNDef> outV;
                    if(targetActiveThingDefData.TryGetValue(pawnDef, out outV)){
                        foreach(BFPTNDef bfp in outV){
                            bfp.tryApplyEffectToTarget(pawn);
                        }
                    }
                    foreach(BFPTNDef bfp in allTargetingBFPTN){
                        bfp.tryApplyEffectToTarget(pawn);
                    }
                }
            }
        }

        public override void MapComponentUpdate() {
            base.MapComponentUpdate();
        }

        public override void MapGenerated() {
            base.MapGenerated();
        }

        public override void MapRemoved() {
            base.MapRemoved();
        }
    }
}
