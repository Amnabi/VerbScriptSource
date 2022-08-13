using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerbScript {
    public class EnumerableEqualityComparer<T, K> : IEqualityComparer<T> where T : IEnumerable<K>{
        public static List<K> SA_F = new List<K>();
        public bool Equals(T x, T y) {
            if(x.Count() != y.Count()){
                return false;
            }
            SA_F.Clear();
            SA_F.AddRange(x);
            int i = 0;
            foreach(K k in y){
                if(((object)SA_F[i]) != (object)k){
                    return false;
                }
                i += 1;
            }
            return true;
        }

        public int GetHashCode(T enu) {
            int cXOR = 0;
            foreach(object obj in enu){
                cXOR ^= hashShift(obj.GetHashCode());
            }
            return cXOR;
        }
        public static int hashShift(int h2) {
            uint h = (uint)h2;
            h ^= (h >> 20) ^ (h >> 12);
            return (int)(h ^ (h >> 7) ^ (h >> 4));
        }
    }

    public static class UniqueSequence<T> {
        //
        public static Dictionary<IEnumerable<T>, int> SA_EnumToIndex = new Dictionary<IEnumerable<T>, int>(new EnumerableEqualityComparer<IEnumerable<T>, T>());
        //public static Dictionary<int, IEnumerable<object>> SA_hashOpenAddress = new Dictionary<int, IEnumerable<object>>();
        public static Dictionary<int, int> SA_indexToRefNum = new Dictionary<int, int>();

        /**public static int hashFrom(IEnumerable<object> enu){
            int cXOR = 0;
            foreach(object obj in enu){
                cXOR ^= hashShift(obj.GetHashCode());
            }
            return cXOR;
        }
        public static int hashShift(int h2) {
            uint h = (uint)h2;
            h ^= (h >> 20) ^ (h >> 12);
            return (int)(h ^ (h >> 7) ^ (h >> 4));
        }**/
        public static int registerAndReturnIndex(IEnumerable<T> enume){
            int indOut;
            if(SA_EnumToIndex.TryGetValue(enume, out indOut)){
                SA_indexToRefNum.setSafe(indOut, SA_indexToRefNum.getSafe(indOut) + 1);
                return indOut;
            }
            indOut = nextIndex();
            SA_EnumToIndex.Add(enume, indOut);
            SA_indexToRefNum.setSafe(indOut, SA_indexToRefNum.getSafe(indOut) + 1);
            return indOut;
        }
        public static void deregister(IEnumerable<T> enume){
            int indOut;
            if(SA_EnumToIndex.TryGetValue(enume, out indOut)){
                int next = SA_indexToRefNum.getSafe(indOut) - 1;
                SA_indexToRefNum.setSafe(indOut, next);
                if(next < 0){
                    throw new Exception("More deregister than register");
                }else if(next == 0){
                    SA_EnumToIndex.Remove(enume);
                }
                return;
            }
            throw new Exception("Tried to deregister an non-existant entry");
        }

        public static List<int> freeIndex = new List<int>();
        public static int SA_DynamicIndex = 0;

        public static int nextIndex(){
            if(freeIndex.Count > 0){
                int i = freeIndex[freeIndex.Count - 1];
                freeIndex.RemoveAt(freeIndex.Count - 1);
                return i;
            }
            return SA_DynamicIndex++;
        }

    }
}
