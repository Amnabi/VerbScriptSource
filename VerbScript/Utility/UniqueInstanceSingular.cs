using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerbScript {
    public static class UniqueInstanceSingular {
        public static Dictionary<stringCH, object> uniqueStringToT = new Dictionary<stringCH, object>();
        public static Dictionary<object, IndexAndRefNum> TUniqueInstanceToIndex = new Dictionary<object, IndexAndRefNum>();
        
        public static int registerUniqueReturnIndex<T>(this T obj){
            return obj.registerUniqueReturnIndex(delegate(T ob){ return ob.ToString().CH(); });
        }
        public static int registerUniqueReturnIndex<T>(this T obj, Func<T, stringCH> uniqueStringFormFunc){
            stringCH uqString = uniqueStringFormFunc(obj);
            if(uniqueStringToT.TryGetValue(uqString, out object output)){
                IndexAndRefNum indRef = TUniqueInstanceToIndex[output];
                indRef.derivativeReference(1);
                return indRef.index;
            }
            uniqueStringToT.Add(uqString, obj);
            int indexNext = nextIndex();
            TUniqueInstanceToIndex.Add(obj, new IndexAndRefNum(1, indexNext));
            return indexNext;
        }     
        
        public static T registerUnique<T>(this T obj){
            return obj.registerUnique(delegate(T ob){ return ob.ToString().CH(); });
        }
        public static T registerUnique<T>(this T obj, Func<T, stringCH> uniqueStringFormFunc){
            stringCH uqString = uniqueStringFormFunc(obj);
            if(uniqueStringToT.TryGetValue(uqString, out object output)){
                TUniqueInstanceToIndex[output].derivativeReference(1);
                return (T)output;
            }
            uniqueStringToT.Add(uqString, obj);
            TUniqueInstanceToIndex.Add(obj, new IndexAndRefNum(1, nextIndex()));
            return obj;
        }        
        public static void deregisterUnique<T>(this T obj){
            obj.deregisterUnique(delegate(T ob){ return ob.ToString().CH(); });
        }
        public static void deregisterUnique<T>(this T obj, Func<T, stringCH> uniqueStringFormFunc){
            stringCH uqString = uniqueStringFormFunc(obj);
            if(uniqueStringToT.TryGetValue(uqString, out object output)){
                int indexOut = TUniqueInstanceToIndex[output].derivativeReference(-1);
                if(indexOut != -1){
                    uniqueStringToT.Remove(uqString);
                    TUniqueInstanceToIndex.Remove(output);
                    SA_FreeIndex.Add(indexOut);
                }
            }else{
                throw new Exception("UniqueInstance Tracker Deregister Desync");
            }
        }
        public static int SA_Index = -1;
        public static List<int> SA_FreeIndex = new List<int>();
        public static int nextIndex(){
            if(SA_FreeIndex.Count > 0){
                int nextInd = SA_FreeIndex[SA_FreeIndex.Count - 1];
                SA_FreeIndex.RemoveAt(SA_FreeIndex.Count - 1);
                return nextInd;
            }
            SA_Index += 1;
            return SA_Index;
        }
    }

    public struct IndexAndRefNum{
        public int referenceNum;
        public int index;
        public IndexAndRefNum(int refNum, int ind){
            referenceNum = refNum;
            index = ind;
        }
        public int derivativeReference(int derivative){
            referenceNum += derivative;
            if(referenceNum <= 0){
                return index;
            }
            return -1;
        }
    }

}
