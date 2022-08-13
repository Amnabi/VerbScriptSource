using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbScript {

    public class AON<T> {
        public static Dictionary<AON<T>, AON<T>> sA_uniqueInstance = new Dictionary<AON<T>, AON<T>>();
        public static List<int> SA_freeID = new List<int>();
        public static int SA_DYNID = -1;
        public static int nextID(){
            if(SA_freeID.Count > 0){
                return SA_freeID.Pop();
            }
            return ++SA_DYNID;
        }

        public int uniqueID;
        public int cachedHash;
        public HashSet<T> hashSetInner = new HashSet<T>();
        
        public override int GetHashCode(){
            return cachedHash;
        }
        public static bool operator ==(AON<T> a, AON<T> b){
            if(object.ReferenceEquals(a, null)){
                return object.ReferenceEquals(b, null);
            }
            return a.Equals(b);
        }
        public static bool operator !=(AON<T> a, AON<T> b){
            if(object.ReferenceEquals(a, null)){
                return !object.ReferenceEquals(b, null);
            }
            return !a.Equals(b);
        }
        public override bool Equals(object obj) {
            if(obj is AON<T> ch){
                if(ch.cachedHash != cachedHash){
                    return false;
                }
                return ch.groupState(this) == GroupState.Same;
            }
            Log.Warning("Wrong comparison!");
            return false;
        }

        public virtual IEnumerable<int> hashIntEnumerable(){
            foreach(T t in hashSetInner){
                yield return t.GetHashCode();
            }
            yield break;
        }

        public void recalculateHash(){
            cachedHash = 0;
            foreach(int btd in hashIntEnumerable()){
                cachedHash ^= hashShift(btd); 
            }
        }
        public AON<T> recalculateHashUniqueInstance(){
            recalculateHash();
            AON<T> ui;
            if(sA_uniqueInstance.TryGetValue(this, out ui)){
                return ui;
            }
            sA_uniqueInstance.Add(this, this);
            uniqueID = nextID();
            return this;
        }

        public static int hashShift(int h2) {
            uint h = (uint)h2;
            h ^= (h >> 20) ^ (h >> 12);
            return (int)(h ^ (h >> 7) ^ (h >> 4));
        }

        public GroupState groupState(AON<T> compareSetB){
            int countA = hashSetInner.Count;
            int countB = compareSetB.hashSetInner.Count;
            bool hasAnyOverlap = false;
            bool Abig = true;
            bool Bbig = true;

            SA_Var293841.Clear();
            SA_Var293841.AddRange(hashSetInner);
            SA_Var293842.Clear();
            SA_Var293842.AddRange(compareSetB.hashSetInner);
            for(int i = 0; i < SA_Var293841.Count; i++){
                if(compareSetB.hashSetInner.Contains(SA_Var293841[i])){
                    hasAnyOverlap = true;
                }else{
                    Bbig = false;
                }
            }
            for(int i = 0; i < SA_Var293842.Count; i++){
                if(hashSetInner.Contains(SA_Var293842[i])){
                    hasAnyOverlap = true;
                }else{
                    Abig = false;
                }
            }

            if(hasAnyOverlap){
                if(Abig){
                    if(Bbig){
                        return GroupState.Same;
                    }else{
                        return GroupState.BinA;
                    }
                }else{
                    if(Bbig){
                        return GroupState.AinB;
                    }else{
                        return GroupState.PartialOverlap;
                    }
                }
            }else{
                return GroupState.NoOverlap;
            }


        }

        public static List<T> SA_Var293841 = new List<T>();
        public static List<T> SA_Var293842 = new List<T>();

    }



}
