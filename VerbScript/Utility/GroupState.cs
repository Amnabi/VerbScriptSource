using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbScript{

    public class GroupStateSet{
        public static List<GroupStateSet> SA_FreeGroupStateSet = new List<GroupStateSet>();
        public static GroupStateSet nextStateSet(){
            if(SA_FreeGroupStateSet.Count > 0){
                return SA_FreeGroupStateSet.Pop();
            }
            return new GroupStateSet();
        }
        public void free(){
            defaultState = 0;//GroupState.Unknown;
            subIndexToState.Clear();
            SA_FreeGroupStateSet.Add(this);
        }

        public int defaultState = 0;//GroupState.Unknown;
        public Dictionary<int, int> subIndexToState = new Dictionary<int, int>();
        public void logTest(){
            string stringStack = "DefaultState " + defaultState;
            foreach(int i in subIndexToState.Keys){
                stringStack += " / " + subIndexToState[i] + "_" +  i;
                stringStack += VC_ThingDef.SA_IndexToDef[i].defName;
            }
            Log.Warning(stringStack);
        }

        public int getStateFor(int subState){
            int outState;
            if(subIndexToState.TryGetValue(subState, out outState)){
                return outState;
            }
            return defaultState;
        }
        public void setStateFor(int subState, int state){
            if(subIndexToState.ContainsKey(subState)){
                subIndexToState.Remove(subState);
            }
            subIndexToState.Add(subState, state);
        }
        public void Copy(GroupStateSet other){
            subIndexToState.Clear();
            defaultState = other.defaultState;
            foreach(int i in other.subIndexToState.Keys){
                subIndexToState.Add(i, other.subIndexToState[i]);
            }
        }

        public static int stateOR(int a, int b){
            if(a == 0 || b == 0){
                return 1;
            }
            return Math.Max(a, b);
        }
        public static int stateAND(int a, int b){
            if(a == 0){
                return b;
            }
            if(b == 0){
                return a;
            }
            return Math.Min(a, b);
        }

        public static HashSet<int> SA_IntMerger = new HashSet<int>();
        public void OR(GroupStateSet other){
            //Log.Warning("OR");
            //this.logTest();
            //other.logTest();
            int defaultStateNext = stateOR(defaultState, other.defaultState);
            SA_IntMerger.Clear();
            SA_IntMerger.AddRange(other.subIndexToState.Keys);
            SA_IntMerger.AddRange(subIndexToState.Keys);
            foreach(int i in SA_IntMerger){
                int b = stateOR(getStateFor(i), other.getStateFor(i));
                setStateFor(i, b);
                if(b == defaultStateNext){
                    subIndexToState.Remove(i);
                }
            }
            defaultState = defaultStateNext;
            //this.logTest();
        }
        public void AND(GroupStateSet other){
            //Log.Warning("AND");
            //this.logTest();
            //other.logTest();
            int defaultStateNext = stateAND(defaultState, other.defaultState);
            SA_IntMerger.Clear();
            SA_IntMerger.AddRange(other.subIndexToState.Keys);
            SA_IntMerger.AddRange(subIndexToState.Keys);
            foreach(int i in SA_IntMerger){
                int b = stateAND(getStateFor(i), other.getStateFor(i));
                setStateFor(i, b);
                if(b == defaultStateNext){
                    subIndexToState.Remove(i);
                }
            }
            defaultState = defaultStateNext;
            //this.logTest();
        }
        /**public void OR_IRRELEVANT(){//handled elsewhere
            defaultState = 1;
            subIndexToState.Clear();
        }
        public void AND_IRRELEVANT(){
        }**/

    }

    public class GroupStateTree{
        public static List<GroupStateTree> SA_FreeGroupStateTree = new List<GroupStateTree>();
        public static GroupStateTree nextStateTree(){
            if(SA_FreeGroupStateTree.Count > 0){
                return SA_FreeGroupStateTree.Pop();
            }
            return new GroupStateTree();
        }
        public void free(){
            state = GroupState.Unknown;
            foreach(GroupStateTree gs in subStateTree){
                gs.free();
            }
            subStateTree.Clear();
            SA_FreeGroupStateTree.Add(this);

            if(this.mergeData != null){
                mergeData.free();
                mergeData = null;
            }
            if(mergeSet != null){
                mergeSet.free();
                mergeSet = null;
            }
            referenceVerbSequence = null;
        }

        public VerbSequence referenceVerbSequence;
        public GroupState state = GroupState.Unknown;
        public List<GroupStateTree> subStateTree = new List<GroupStateTree>();

        public GroupStateTree mergeData;
        public GroupStateSet mergeSet;

        public void checkForErrors(string prefix){
            if(referenceVerbSequence is VC_GroupableAON){
                if(subStateTree.Count == 0){
                    Log.Warning(prefix + " AON node has no subStates!");
                }
            }
            for(int i = 0; i < subStateTree.Count; i++){
                subStateTree[i].checkForErrors(prefix);
            }
        }

        public void read(VerbSequence ve){
            /**if(ve is VerbScope vsc && vsc.scopeLeftType() == ScopeLeftType.Root){
                Log.Error("Scope left type Root is being read from state tree! This should not happen!");
            }**/
            referenceVerbSequence = ve;
            state = GroupState.Unknown;
            if(ve is VC_GroupableAON){
                if(ve is VC_GroupableAON_OR isor){
                    foreach(VerbSequence vEff in isor.aon.hashSetInner){
                        if(vEff is VerbScope vscope){
                            foreach(VerbSequence vskip in vscope.skipScopeRightTypeEnumerable()){
                                GroupStateTree gst = nextStateTree();
                                subStateTree.Add(gst);
                                gst.read(vskip);
                            }
                        }else{
                            GroupStateTree gst = nextStateTree();
                            subStateTree.Add(gst);
                            gst.read(vEff);
                        }
                    }
                }else if(ve is VC_GroupableAON_AND isor2){
                    foreach(VerbSequence vEff in isor2.aon.hashSetInner){
                        if(vEff is VerbScope vscope){
                            foreach(VerbSequence vskip in vscope.skipScopeRightTypeEnumerable()){
                                GroupStateTree gst = nextStateTree();
                                subStateTree.Add(gst);
                                gst.read(vskip);
                            }
                        }else{
                            GroupStateTree gst = nextStateTree();
                            subStateTree.Add(gst);
                            gst.read(vEff);
                        }
                    }
                }else if(ve is VC_GroupableAON_NOT isor3){
                    foreach(VerbSequence vEff in isor3.aon.hashSetInner){
                        if(vEff is VerbScope vscope){
                            foreach(VerbSequence vskip in vscope.skipScopeRightTypeEnumerable()){
                                GroupStateTree gst = nextStateTree();
                                subStateTree.Add(gst);
                                gst.read(vskip);
                            }
                        }else{
                            GroupStateTree gst = nextStateTree();
                            subStateTree.Add(gst);
                            gst.read(vEff);
                        }
                    }
                }else{
                    throw new NotImplementedException();
                }
            }else{
            }
        }
        //ASSUME AND
        public void read(List<VerbSequence> ve){
            referenceVerbSequence = null;
            state = GroupState.Unknown;
            for(int i = 0; i < ve.Count; i++){
                if(ve[i] is VerbScope vscope){
                    foreach(VerbSequence vskip in vscope.skipScopeRightTypeEnumerable()){
                        GroupStateTree gst = nextStateTree();
                        subStateTree.Add(gst);
                        gst.read(vskip);
                    }
                }else{
                    GroupStateTree gst = nextStateTree();
                    subStateTree.Add(gst);
                    gst.read(ve[i]);
                }
            }
        }
        
        public void mergeNOT(){
            state = GroupState.PartialOverlap;
            for(int i = 0; i < subStateTree.Count; i++){
                subStateTree[i].mergeNOT();
            }
        }
        public void merge(GroupStateTree treeOther, bool orTrueAndFalse, bool leftTrueRightFalse){
            /**if(orTrueAndFalse){
                state = GroupStateUtility.GSLogicMax(state, treeOther.state, leftTrueRightFalse);
            }else{
                state = GroupStateUtility.GSLogicMin(state, treeOther.state, leftTrueRightFalse);
            }**/

            state = GroupStateUtility.GSLogic(state, treeOther.state, orTrueAndFalse, leftTrueRightFalse);
            for(int i = 0; i < subStateTree.Count; i++){
                subStateTree[i].merge(treeOther.subStateTree[i], orTrueAndFalse, leftTrueRightFalse);
            }
        }
        public void copyFrom(GroupStateTree treeOther, bool generateTree){
            state = treeOther.state;
            referenceVerbSequence = treeOther.referenceVerbSequence;
            if(generateTree){
                for(int i = 0; i < treeOther.subStateTree.Count; i++){
                    GroupStateTree gst = nextStateTree();
                    subStateTree.Add(gst);
                }
            }
            for(int i = 0; i < subStateTree.Count; i++){
                subStateTree[i].copyFrom(treeOther.subStateTree[i], generateTree);
            }
        }

        public void breadthFirstAction(Action<VerbSequence, GroupStateTree> veAction){
            veAction(referenceVerbSequence, this);
            for(int i = 0; i < subStateTree.Count; i++){
                subStateTree[i].breadthFirstAction(veAction);
            }
        }
        public void depthFirstAction(Action<VerbSequence, GroupStateTree> veAction){
            for(int i = 0; i < subStateTree.Count; i++){
                subStateTree[i].depthFirstAction(veAction);
            }
            veAction(referenceVerbSequence, this);
        }
        public void compile(bool leftTrueRightFalse = true){
            compileInner(leftTrueRightFalse);
            this.mergeData.compileInner(false);
            state = mergeData.state;
        }
        public void compileInner(bool leftTrueRightFalse = true){
            for(int i = 0; i < subStateTree.Count; i++){
                subStateTree[i].compileInner(leftTrueRightFalse);
            }
            if(subStateTree.Count == 0){
                if(this.mergeData != null){
                    mergeData.compileInner(false);
                    state = mergeData.state;
                    if(state == GroupState.Unknown){
                        Log.Error("Compiled set compare result returned unknown.");
                    }
                }else{
                    if(state == GroupState.Unknown){
                        Log.Error("Leaf node state is unknown! This will cause problems in compilation");
                        Log.Error("InputVar now " + this.referenceVerbSequence + " / " + leftTrueRightFalse);
                    }
                }
            }else{                
                if(referenceVerbSequence is VC_GroupableAON isor){
                    bool isorIsNOT = isor is VC_GroupableAON_NOT;
                    if(this.mergeData != null){
                        if(isorIsNOT){
                            mergeData.mergeNOT();
                        }else{
                            this.mergeData.copyFrom(subStateTree[0].mergeData, false);
                            //state = subStateTree[0].state;
                            for(int i = 1; i < subStateTree.Count; i++){
                                this.mergeData.merge(subStateTree[i].mergeData, isor.orTrueAndFalse(), leftTrueRightFalse);
                                //state = GroupStateUtility.GSLogic(state, subStateTree[i].state, isor.orTrueAndFalse(), leftTrueRightFalse);
                            }
                        }
                    }else{
                        if(isorIsNOT){
                            state = GroupState.PartialOverlap;
                        }else{
                            state = subStateTree[0].state;
                            for(int i = 1; i < subStateTree.Count; i++){
                                state = GroupStateUtility.GSLogic(state, subStateTree[i].state, isor.orTrueAndFalse(), leftTrueRightFalse);
                            }
                        }
                    }
                }else{
                    throw new NotImplementedException();
                }
            }
        }
        
        public void compileSet<T>(bool leftTrueRightFalse = true) where T : VerbSequence{
            compileSetInner<T>(leftTrueRightFalse);
            //this.mergeSet.compileInner(false);
            //state = mergeData.state;
        }
        public void compileSetInner<T>(bool leftTrueRightFalse = true){
            for(int i = 0; i < subStateTree.Count; i++){
                subStateTree[i].compileSetInner<T>(leftTrueRightFalse);
            }
            if(subStateTree.Count == 0){
                if(this.mergeSet != null){
                    if(typeof(T).IsAssignableFrom(referenceVerbSequence.GetType())){
                        mergeSet.defaultState = -1;
                        mergeSet.setStateFor(this.referenceVerbSequence.uniqueSubIDFromContent(), 1);

                    }else{
                        mergeSet.defaultState = 0; //irrelevant
                    }
                }else{
                    Log.Warning("This most likely shouldnt happen 2");
                }
            }else{                
                if(referenceVerbSequence == null){ 
                    mergeSet.Copy(subStateTree[0].mergeSet);
                    for(int i = 1; i < subStateTree.Count; i++){
                        mergeSet.AND(subStateTree[i].mergeSet);
                    }
                }else if(referenceVerbSequence is VC_GroupableAON isor){
                    mergeSet.Copy(subStateTree[0].mergeSet);
                    if(isor is VC_GroupableAON_NOT){
                        mergeSet.defaultState = 0;
                        mergeSet.subIndexToState.Clear();
                    }
                    else if(isor.orTrueAndFalse()){
                        for(int i = 1; i < subStateTree.Count; i++){
                            mergeSet.OR(subStateTree[i].mergeSet);
                        }
                    }else{
                        for(int i = 1; i < subStateTree.Count; i++){
                            mergeSet.AND(subStateTree[i].mergeSet);
                        }
                    }
                }else{
                    throw new NotImplementedException();
                }
            }
        }

    }

    public static class GroupStateUtility{
        public static GroupStateTree SA_gstRight;
        public static Action<VerbSequence, GroupStateTree> SA_Init = delegate(VerbSequence ve, GroupStateTree gs) {
                //if(!(ve is VS_GroupableAON)){
                    gs.mergeData = GroupStateTree.nextStateTree();
                    gs.mergeData.copyFrom(SA_gstRight, true);
                    //SA_gstRight.checkForErrors("Prefix GSTR ");
                    //gs.mergeData.checkForErrors("Prefix MERGD ");
                //}
            };
        public static GroupStateSet SA_GroupStateSet = GroupStateSet.nextStateSet();
        public static Action<VerbSequence, GroupStateTree> SA_Set_Init = delegate(VerbSequence ve, GroupStateTree gs) {
                gs.mergeSet = GroupStateSet.nextStateSet();
            };
        
        public static GroupState mirror(this GroupState gs){
            if(gs == GroupState.AinB){
                return GroupState.BinA;
            }else if(gs == GroupState.BinA){
                return GroupState.AinB;
            }
            return gs;
        }

        public static GroupState compareTwo(VerbSequence veLeft, VerbSequence veRight){
            /**if(veLeft is VerbScope || veRight is VerbScope){
                throw new Exception("What the fucj");
                return GroupState.Unknown;
            }**/
            string A = SA_LastStr;
            GroupState gsA = compareTwoInner(veLeft, veRight);
            string B = SA_LastStr;
            GroupState gsB = compareTwoInner(veRight, veLeft).mirror();
            if(gsA == gsB){
                if(VerbData.DebugLog)Log.Warning(A);
                return gsA;
            }
            if(VerbData.DebugLog)Log.Warning("AB Desync " + veLeft.uniqueID + " / " + veRight.uniqueID);
            Log.Warning(A);
            Log.Warning(B);
            if(gsA == GroupState.BinA || gsA == GroupState.AinB){
                if(VerbData.DebugLog)Log.Warning("Compare State Final " + gsA);
                return gsA;
            }
            if(gsB == GroupState.BinA || gsB == GroupState.AinB){
                if(VerbData.DebugLog)Log.Warning("Compare State Final Mirror " + gsB);
                return gsB;
            }
            Log.Warning("Unknown state");
            return GroupState.Unknown;
            /**if(GSLogicTrueLeftFalseRight(gsA, gsB, true)){
                Log.Warning("Compare State Final " + gsA.mirror());
                return gsA.mirror();
            }else{
                Log.Warning("Compare State Final " + gsB);
                return gsB;
            }**/
            //Log.Warning("Mismatch " + gsA + " / " + gsB + " _ " + GSLogicMin(gsA, gsB, true) + " / " + GSLogicMax(gsB, gsA, true));
            //return GSLogicMin(gsA, gsB, true);
        }
        public static string SA_LastStr = "";
        private static GroupState compareTwoInner(VerbSequence veLeft, VerbSequence veRight){
            //Log.Warning("--------------");
            GroupStateTree gstLeft = GroupStateTree.nextStateTree();
            gstLeft.read(veLeft);
            SA_gstRight = GroupStateTree.nextStateTree();
            SA_gstRight.read(veRight);
            gstLeft.breadthFirstAction(SA_Init);
            gstLeft.breadthFirstAction(delegate(VerbSequence ve, GroupStateTree gs) {
                if(!(ve is VC_GroupableAON)){
                    gs.mergeData.breadthFirstAction(delegate(VerbSequence ve2, GroupStateTree gs2) {
                        if(!(ve2 is VC_GroupableAON)){
                            gs2.state = ve.func198273(gs2.referenceVerbSequence).convertGS();
                        }
                    });
                }
            });
            gstLeft.compile();
            GroupState gsEnd = gstLeft.state.deconvertGS();

            gstLeft.free();
            SA_gstRight.free();

            SA_LastStr = "Compare State " + gsEnd + " " + veLeft.uniqueID.text + " / " + veRight.uniqueID.text;

            return gsEnd;
        }
        public static GroupStateSet compareSet<T>(List<VerbSequence> veLeft) where T : VerbSequence{
            //Log.Warning("--------------");
            GroupStateTree gstLeft = GroupStateTree.nextStateTree();
            gstLeft.read(veLeft);
            gstLeft.breadthFirstAction(SA_Set_Init);
            gstLeft.compileSet<T>();
            SA_GroupStateSet.Copy(gstLeft.mergeSet);
            gstLeft.free();
            return SA_GroupStateSet;
        }

        public static GroupState convertGS(this GroupState gs){
            if(gs == GroupState.NoOverlap){
                return GroupState.PartialOverlap;
            }
            return gs;
        }
        public static GroupState deconvertGS(this GroupState gs){
            if(gs == GroupState.PartialOverlap){
                return GroupState.NoOverlap;
            }
            return gs;
        }
        
        public static int priority(GroupState state, bool leftTrueRightFalse){
            if(leftTrueRightFalse){
                switch(state){
                    case GroupState.AinB:{
                        return 0;
                    }
                    case GroupState.BinA:{
                        return 3;
                    }
                    case GroupState.NoOverlap:{
                        return 1;
                    }
                    case GroupState.PartialOverlap:{
                        return 1;
                    }
                    case GroupState.Same:{
                        return 2;
                    }
                }
            }else{
                switch(state){
                    case GroupState.AinB:{
                        return 3;
                    }
                    case GroupState.BinA:{
                        return 0;
                    }
                    case GroupState.NoOverlap:{
                        return 1;
                    }
                    case GroupState.PartialOverlap:{
                        return 1;
                    }
                    case GroupState.Same:{
                        return 2;
                    }
                }
            }
            return -1;
        }
        public static GroupState GSLogicMin(GroupState stateLeft, GroupState stateRight, bool leftTrueRightFalse){
            if(priority(stateLeft, leftTrueRightFalse) > priority(stateRight, leftTrueRightFalse)){
                return stateRight;
            }else{
                return stateLeft;
            }
        }
        public static GroupState GSLogicMax(GroupState stateLeft, GroupState stateRight, bool leftTrueRightFalse){
            if(priority(stateLeft, leftTrueRightFalse) > priority(stateRight, leftTrueRightFalse)){
                return stateLeft;
            }else{
                return stateRight;
            }
        }
        public static bool GSLogicTrueLeftFalseRight(GroupState stateLeft, GroupState stateRight, bool leftTrueRightFalse){
            if(priority(stateLeft, leftTrueRightFalse) > priority(stateRight, leftTrueRightFalse)){
                return true;
            }else{
                return false;
            }
        }

        public static GroupState GSLogic(GroupState stateLeft, GroupState stateRight, bool orTrueAndFalse, bool leftTrueRightFalse){
            if(stateLeft == GroupState.Unknown || stateRight == GroupState.Unknown){
                Log.Warning("This should not happen! Comparing two unknown states!");
                return GroupState.Unknown;
            }
            bool state2 = orTrueAndFalse ^ leftTrueRightFalse;
            if(state2){
                switch(stateLeft){
                    case GroupState.BinA:{
                        switch(stateRight){
                            case GroupState.BinA:{
                                return GroupState.BinA;
                            }
                            case GroupState.AinB:{
                                return GroupState.AinB;
                            }
                            case GroupState.PartialOverlap:{
                                return GroupState.PartialOverlap;
                            }
                            case GroupState.Same:{
                                return GroupState.Same;
                            }
                        }
                        throw new NotImplementedException();
                    }
                    case GroupState.AinB:{
                        switch(stateRight){
                            case GroupState.BinA:{
                                return GroupState.AinB;
                            }
                            case GroupState.AinB:{
                                return GroupState.AinB;
                            }
                            case GroupState.PartialOverlap:{
                                return GroupState.AinB;
                            }
                            case GroupState.Same:{
                                return GroupState.AinB;
                            }
                        }
                        throw new NotImplementedException();
                    }
                    case GroupState.PartialOverlap:{
                        switch(stateRight){
                            case GroupState.BinA:{
                                return GroupState.PartialOverlap;
                            }
                            case GroupState.AinB:{
                                return GroupState.AinB;
                            }
                            case GroupState.PartialOverlap:{
                                return GroupState.PartialOverlap;
                            }
                            case GroupState.Same:{
                                return GroupState.AinB;
                            }
                        }
                        throw new NotImplementedException();
                    }
                    case GroupState.Same:{
                        switch(stateRight){
                            case GroupState.BinA:{
                                return GroupState.Same;
                            }
                            case GroupState.AinB:{
                                return GroupState.AinB;
                            }
                            case GroupState.PartialOverlap:{
                                return GroupState.AinB;
                            }
                            case GroupState.Same:{
                                return GroupState.Same;
                            }
                        }
                        throw new NotImplementedException();
                    }
                    default:{
                        throw new NotImplementedException();
                    }
                }
            }else{
                switch(stateLeft){
                    case GroupState.BinA:{
                        switch(stateRight){
                            case GroupState.BinA:{
                                return GroupState.BinA;
                            }
                            case GroupState.AinB:{
                                return GroupState.BinA;
                            }
                            case GroupState.PartialOverlap:{
                                return GroupState.BinA;
                            }
                            case GroupState.Same:{
                                return GroupState.BinA;
                            }
                        }
                        throw new NotImplementedException();
                    }
                    case GroupState.AinB:{
                        switch(stateRight){
                            case GroupState.BinA:{
                                return GroupState.BinA;
                            }
                            case GroupState.AinB:{
                                return GroupState.AinB;
                            }
                            case GroupState.PartialOverlap:{
                                return GroupState.PartialOverlap;
                            }
                            case GroupState.Same:{
                                return GroupState.Same;
                            }
                        }
                        throw new NotImplementedException();
                    }
                    case GroupState.PartialOverlap:{
                        switch(stateRight){
                            case GroupState.BinA:{
                                return GroupState.BinA;
                            }
                            case GroupState.AinB:{
                                return GroupState.PartialOverlap;
                            }
                            case GroupState.PartialOverlap:{
                                return GroupState.PartialOverlap;
                            }
                            case GroupState.Same:{
                                return GroupState.BinA;
                            }
                        }
                        throw new NotImplementedException();
                    }
                    case GroupState.Same:{
                        switch(stateRight){
                            case GroupState.BinA:{
                                return GroupState.BinA;
                            }
                            case GroupState.AinB:{
                                return GroupState.Same;
                            }
                            case GroupState.PartialOverlap:{
                                return GroupState.BinA;
                            }
                            case GroupState.Same:{
                                return GroupState.Same;
                            }
                        }
                        throw new NotImplementedException();
                    }
                    default:{
                        throw new NotImplementedException();
                    }
                }
            }

        }

    }
    public enum GroupState{
        NoOverlap,
        PartialOverlap,
        Same,
        AinB,
        BinA,
        Unknown
    }
    
}
