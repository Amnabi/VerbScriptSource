using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerbScript {
    
    public class FreePoolObjectHandler<T>{
        static FreePoolObjectHandler(){
        }

        public virtual T CreateObject(){
            return (T)Activator.CreateInstance(typeof(T));
        }
        public virtual void Clear(T t){

        }
    }

    public static class FreePool<T>{
        public static List<T> SA_FreePool = new List<T>();
        public static FreePoolObjectHandler<T> freePoolObjectHandler = new FreePoolObjectHandler<T>();
        public static T next(){
            if(SA_FreePool.Count() > 0){
                T tOut = SA_FreePool[SA_FreePool.Count() - 1];
                SA_FreePool.RemoveAt(SA_FreePool.Count() - 1);
                return tOut;
            }
            return freePoolObjectHandler.CreateObject();
        }
        public static void free(T t){
            freePoolObjectHandler.Clear(t);
            SA_FreePool.Add(t);
        }


    }

}
