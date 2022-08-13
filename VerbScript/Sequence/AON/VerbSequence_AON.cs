using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;


namespace VerbScript {
    
    public abstract class VC_GroupableAON : VS_Groupable{
        public virtual bool orTrueAndFalse(){
            return true;
        }
    }

    public class VC_GroupableAON_NOT : VC_GroupableAON{
        [IndexedLoad(0)]
        public VerbSequence verbSequence;
        public AON<VerbSequence> aon { get{ if(null == aonPrivate){ aonPrivate = new AON<VerbSequence>(); aonPrivate.hashSetInner.Add(verbSequence); aonPrivate = aonPrivate.recalculateHashUniqueInstance();
            } return aonPrivate; } }
        public override void RegisterAllTypes(VerbRootQD destination){
            verbSequence.RegisterAllTypes(destination);
            base.RegisterAllTypes(destination);
        }
        private AON<VerbSequence> aonPrivate;
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            verbSequence = verbSequence.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            verbSequence.appendID();
            SA_StringBuilder.Append("]");
        }
        public override long PG_groupIndex(){
            return aon.uniqueID;//base.groupIndex();//return dynaGenGID;
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            yield return verbSequence.quickEvaluateAsBool(context) ? false : true;
            /**
            object obj = verbSequence.quickEvaluate(context).singular();
            if(obj is int a){
                if(a == verbSequence.uniqueSubIDFromContent()){
                    yield return -1;
                    yield break;
                }else{
                    yield return 0;
                    yield break;
                }
            }else if(obj is bool b){
                if(b){
                    yield return -1;
                    yield break;
                }else{
                    yield return 0;
                    yield break;
                }
            }
            yield return 0;**/
        }
        
        //assume vegaon is single element
        protected override GroupState groupStateCompareSingular(VerbSequence vegaon){
            throw new NotImplementedException("VS_GroupableAON groupStateCompare");//return GroupState.Same;
        }
    }

    public class VC_GroupableAON_OR : VC_GroupableAON{
        [ListLoad(typeof(VerbSequence))]
        public List<VerbSequence> verbSequences = new List<VerbSequence>(); 
        public AON<VerbSequence> aon { get{ if(null == aonPrivate){ aonPrivate = new AON<VerbSequence>(); aonPrivate.hashSetInner.AddRange(verbSequences); aonPrivate = aonPrivate.recalculateHashUniqueInstance();
            } return aonPrivate; } }
        public override bool orTrueAndFalse(){return true;}
        public override void RegisterAllTypes(VerbRootQD destination){
            foreach(VerbSequence ve in verbSequences){
                ve.RegisterAllTypes(destination);
            }
            base.RegisterAllTypes(destination);
        }
        private AON<VerbSequence> aonPrivate;
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            for(int i = 0; i < verbSequences.Count; i++){
                if(verbSequences[i] is VC_GroupableAON_OR orCas){
                    verbSequences.RemoveAt(i);
                    i -= 1;
                    verbSequences.AddRange(orCas.verbSequences);
                    //Log.Warning("OR inside OR! Merging");
                }
            }
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i] = verbSequences[i].registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            }
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            foreach(VerbSequence jd in aon.hashSetInner){
                jd.appendID();
            }
            SA_StringBuilder.Append("]");
        }
        public override long PG_groupIndex(){
            return aon.uniqueID;//base.groupIndex();//return dynaGenGID;
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            foreach(VerbSequence ve in aon.hashSetInner){
                foreach(object obj in ve.quickEvaluate(context)){
                    if(obj is bool b){
                        if(b){
                            yield return true;
                            yield break;
                        }
                    }else{
                        throw new Exception("Unknown Type");
                    }
                }
            }
            yield return false;
        }
        
        //assume vegaon is single element
        protected override GroupState groupStateCompareSingular(VerbSequence vegaon){
            throw new NotImplementedException("VS_GroupableAON groupStateCompare");//return GroupState.Same;
        }
    }
    public class VC_GroupableAON_AND : VC_GroupableAON{
        [ListLoad(typeof(VerbSequence))]
        public List<VerbSequence> verbSequences = new List<VerbSequence>(); public AON<VerbSequence> aon { get{ if(null == aonPrivate){ aonPrivate = new AON<VerbSequence>(); aonPrivate.hashSetInner.AddRange(verbSequences); aonPrivate = aonPrivate.recalculateHashUniqueInstance();
            } return aonPrivate; } }
        public override bool orTrueAndFalse(){return false;}
        public override void RegisterAllTypes(VerbRootQD destination){
            foreach(VerbSequence ve in verbSequences){
                ve.RegisterAllTypes(destination);
            }
            base.RegisterAllTypes(destination);
        }
        private AON<VerbSequence> aonPrivate;
        public override VerbSequence registerAllSubVerbSequencesAndReturn(List<VerbScope> verbScopesParent, ScopeLeftType leftHand){
            for(int i = 0; i < verbSequences.Count; i++){
                if(verbSequences[i] is VC_GroupableAON_AND orCas){
                    verbSequences.RemoveAt(i);
                    i -= 1;
                    verbSequences.AddRange(orCas.verbSequences);
                    //Log.Warning("AND inside AND! Merging");
                }
            }
            for(int i = 0; i < verbSequences.Count; i++){
                verbSequences[i] = verbSequences[i].registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand.mask(ScopeLeftType.CPE));
            }
            return base.registerAllSubVerbSequencesAndReturn(verbScopesParent, leftHand);
        }
        public override void appendID(){
            base.appendID();
            SA_StringBuilder.Append("[");
            foreach(VerbSequence jd in aon.hashSetInner){
                jd.appendID();
            }
            SA_StringBuilder.Append("]");
        }
        public override long PG_groupIndex(){
            return aon.uniqueID;//base.groupIndex();//return dynaGenGID;
        }
        public override int uniqueSubIDFromContent(){
            return 0;
        }
        public override IEnumerable<object> evaluate(ExecuteStackContext context){
            foreach(VerbSequence ve in aon.hashSetInner){
                foreach(object obj in ve.quickEvaluate(context)){
                    if(obj is bool b){
                        if(!b){
                            yield return false;
                            yield break;
                        }
                    }else{
                        throw new Exception("Unknown Type");
                    }
                }
            }
            yield return true;
        }
        
        //assume vegaon is single element
        protected override GroupState groupStateCompareSingular(VerbSequence vegaon){
            throw new NotImplementedException("VS_GroupableAON groupStateCompare");//return GroupState.Same;
        }
    }
}
