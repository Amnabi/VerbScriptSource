﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace BFPTN {
	public class TraitAndDegreeClass{
		public TraitDef traitDef;
		public int degree;

        public override bool Equals(object obj) {
            return (obj is TraitAndDegreeClass t) && t.traitDef == traitDef && t.degree == degree;
        }
        public override int GetHashCode() {
            return (traitDef.index << 8) + degree;
        }

        public TraitAndDegree structForm(){
            TraitAndDegree da = new TraitAndDegree();
            da.traitDef = traitDef;
            da.degree = degree;
            return da;
        }

    }
	public struct TraitAndDegree{
		public TraitDef traitDef;
		public int degree;

        public override bool Equals(object obj) {
            return (obj is TraitAndDegree t) && t.traitDef == traitDef && t.degree == degree;
        }
        public override int GetHashCode() {
            return (traitDef.index << 8) + degree;
        }

    }
    public class BFPTNDef : Def{
        //List Form
        public List<TraitAndDegree> requiredTraitsPostLoad = new List<TraitAndDegree>();
        public List<TraitAndDegreeClass> requiredTraits = new List<TraitAndDegreeClass>();
        public List<int> requiredTraitNumber = new List<int>();
        public List<ThingDef> requiredPawnDefs = new List<ThingDef>();
        public List<int> requiredPawnNumber = new List<int>();
        
        public List<ThingDef> requiredPawnDefsLesserOrEqual = new List<ThingDef>();
        public List<int> requiredPawnNumberLesserOrEqual = new List<int>();

        public List<ThingDef> targetPawnDefs = new List<ThingDef>();
        public bool applyToAllInstead = false;
        public HediffDef hediffToApply;
        public float priority;
        public List<BFPTNDef> prerequisiteDefs = new List<BFPTNDef>();

        //Autogenerated
        public HashSet<ThingDef> HS_requiredPawnDefsLesserOrEqual;
        public HashSet<ThingDef> HS_requiredPawnDefs;
        public HashSet<TraitAndDegree> HS_requiredTraits;
        public HashSet<ThingDef> HS_targetPawnDefs;
        public int exclusiveTag;
        //public HashSet<int> exclusiveTags;
        public List<BFPTNDef> childDefs = new List<BFPTNDef>();
        
        public void recursiveDown(int ID, bool stopOnMultiparent = true){
            if(stopOnMultiparent && !prerequisiteDefs.NullOrEmpty()){
                return;
            }
            exclusiveTag = ID;
            //exclusiveTags.Add(ID);
            foreach(BFPTNDef bfp in childDefs){
                bfp.recursiveDown(ID, stopOnMultiparent);
            }
        }

        public void initializeOptimizations2(){
            if(prerequisiteDefs.Count == 0){//case Root
                recursiveDown(nextID());
            }

        }
        public void initializeOptimizations(){
            HS_requiredPawnDefsLesserOrEqual = new HashSet<ThingDef>();
            HS_requiredPawnDefs = new HashSet<ThingDef>();
            HS_targetPawnDefs = new HashSet<ThingDef>();
            HS_requiredTraits = new HashSet<TraitAndDegree>();

            HS_requiredPawnDefsLesserOrEqual.AddRange(requiredPawnDefsLesserOrEqual);
            HS_requiredPawnDefs.AddRange(requiredPawnDefs);
            HS_targetPawnDefs.AddRange(targetPawnDefs);

            foreach(TraitAndDegreeClass claz in requiredTraits){
                requiredTraitsPostLoad.Add(claz.structForm());
            }

            HS_requiredTraits.AddRange(requiredTraitsPostLoad);
            
            foreach(ThingDef td in requiredPawnDefsLesserOrEqual){
                func234246(td, this);
                if(HS_requiredPawnDefs.Count == 0 && HS_requiredTraits.Count == 0){
                    SA_HS_CheckRegardless.Add(this);
                }
            }
            foreach(ThingDef td in HS_requiredPawnDefs){
                func234246(td, this);
            }
            foreach(TraitAndDegree td in HS_requiredTraits){
                func234247(td, this);
            }

            foreach(BFPTNDef bfptr in prerequisiteDefs){
                bfptr.childDefs.Add(this);
            }

            if(hediffToApply == null){
                Log.Warning("hediffToApply is null! This def is pointless!");
            }
            if(requiredPawnDefsLesserOrEqual.NullOrEmpty() && requiredPawnDefs.NullOrEmpty() && requiredTraits.NullOrEmpty()){
                Log.Warning("requiredPawnDefs is empty! This def is pointless!");
            }
            if(targetPawnDefs.NullOrEmpty() && !applyToAllInstead){
                Log.Warning("targetPawnDefs is empty! This def is pointless!");
            }
            if(prerequisiteDefs.Count > 1){
                Log.Warning("prerequisiteDefs has more than 1 element! This case may not work properly yet!");
            }

        }
        
        public bool activeCheck(MapComponent_BFPTN mapComp){
            for(int i = 0; i < requiredPawnDefs.Count; i++){
                ThingDef td = requiredPawnDefs[i];
                ThingDefKeyExtendedData tdk = mapComp.tryGetData(td, false);
                if(tdk == null || tdk.count < requiredPawnNumber[i]){
                    return false;
                }
            }
            for(int i = 0; i < requiredPawnDefsLesserOrEqual.Count; i++){
                ThingDef td = requiredPawnDefsLesserOrEqual[i];
                ThingDefKeyExtendedData tdk = mapComp.tryGetData(td, false);
                if(tdk != null && tdk.count > requiredPawnNumberLesserOrEqual[i]){
                    return false;
                }
            }
            for(int i = 0; i < requiredTraitsPostLoad.Count; i++){
                TraitAndDegree td = requiredTraitsPostLoad[i];
                TraitAndDegreeKeyExtendedData tdk = mapComp.tryGetData(td, false);
                if(tdk == null || tdk.count < requiredTraitNumber[i]){
                    return false;
                }
            }
            return true;
        }

        public void tryApplyEffectToTarget(Pawn target){
            Hediff hDef = target.health.hediffSet.GetFirstHediffOfDef(hediffToApply);
            if(hDef != null){
                hDef.Severity = 1; //this.cachePower;
                HediffComp_Disappears hcd = hDef.TryGetComp<HediffComp_Disappears>();
                hcd.ticksToDisappear = BFPTNSettings.ticksPerCheck;
            }else{
                Hediff hediff = HediffMaker.MakeHediff(hediffToApply, target);
                hediff.Severity = 1;// this.cachePower;
                target.health.AddHediff(hediff);
                HediffComp_Disappears hcd = hediff.TryGetComp<HediffComp_Disappears>();
                hcd.ticksToDisappear = BFPTNSettings.ticksPerCheck;
            }
        }

        /**public void calculatePower(MapComponent_BFPTN mapComp){
            cachePower = 0;
            foreach(ThingDef td in HS_requiredPawnDefs){
                ThingDefKeyExtendedData tdk = mapComp.tryGetData(td, false);
                if(tdk != null){
                    cachePower += tdk.count;
                }
            }
        }
        public float cachePower = -1;**/

        //Static Optimizations
        public static int SA_nextID = -1;
        //public static Dictionary<ThingDef, List<BFPTNDef>> SA_RequiredThingDefToBFPTN = new Dictionary<ThingDef, List<BFPTNDef>>();
        public static Dictionary<ThingDef, HashSet<BFPTNDef>> SA_HS_RequiredThingDefToBFPTN = new Dictionary<ThingDef, HashSet<BFPTNDef>>();
        public static Dictionary<TraitAndDegree, HashSet<BFPTNDef>> SA_HS_RequiredTraitAndDegreeToBFPTN = new Dictionary<TraitAndDegree, HashSet<BFPTNDef>>();
        public static HashSet<BFPTNDef> SA_HS_CheckRegardless = new HashSet<BFPTNDef>();

        public static int nextID(){
            SA_nextID += 1;
            return SA_nextID;
        }
        public static void func234246(ThingDef td, BFPTNDef bfp){
            if(!SA_HS_RequiredThingDefToBFPTN.ContainsKey(td)){
                SA_HS_RequiredThingDefToBFPTN.Add(td, new HashSet<BFPTNDef>());
            }
            SA_HS_RequiredThingDefToBFPTN[td].Add(bfp);
        }
        public static void func234247(TraitAndDegree td, BFPTNDef bfp){
            if(!SA_HS_RequiredTraitAndDegreeToBFPTN.ContainsKey(td)){
                //SA_RequiredThingDefToBFPTN.Add(td, new List<BFPTNDef>());
                SA_HS_RequiredTraitAndDegreeToBFPTN.Add(td, new HashSet<BFPTNDef>());
            }
            //SA_RequiredThingDefToBFPTN[td].Add(bfp);
            SA_HS_RequiredTraitAndDegreeToBFPTN[td].Add(bfp);
        }

    }
}