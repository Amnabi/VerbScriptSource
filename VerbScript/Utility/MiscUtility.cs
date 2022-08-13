using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace VerbScript {
    public static class MathUtility{
        public static float modulo(this float number, float modulo){
            return ((number % modulo) + modulo) % modulo;
        }
    }
    public static class MiscUtility {
        public static Dictionary<string, Type> SA_sToType = new Dictionary<string, Type>();
        public static Type typeFromString(string typeString){
            if(SA_sToType.TryGetValue(typeString, out Type v)){
                return v;
            }
			Type type2 = Type.GetType(typeString, false, true);
			if (type2 != null){
                SA_sToType.Add(typeString, type2);
				return type2;
			}
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()){
				Type type = assembly.GetType(typeString, false, true);
				if (type != null){
                    SA_sToType.Add(typeString, type);
					return type;
				}
			}
            SA_sToType.Add(typeString, null);
            return null;
        }
    }
    public static class DictionaryUtility{
        public static K getSafe<T, K>(this Dictionary<T, K> d, T key){
            return d.getSafe(key, default(K));
        }
        public static K getSafe<T, K>(this Dictionary<T, K> d, T key, K defaultValue){
            K po;
            if(d.TryGetValue(key, out po)){
                return po;
            }
            return defaultValue;
        }
        public static K getInitSafe<T, K>(this Dictionary<T, K> d, T key, Func<K> defaultValueSetter){
            K po;
            if(d.TryGetValue(key, out po)){
                return po;
            }
            K kValue = defaultValueSetter();
            d.Add(key, kValue);
            return kValue;
        }
        public static void setSafe<T, K>(this Dictionary<T, K> d, T key, K value, bool removeIfKeyIsDefault = true, K defaultValue = default(K)){
            if(d.ContainsKey(key)){
                d.Remove(key);
            }
            if(removeIfKeyIsDefault){
                if((object)value == (object)defaultValue){
                    return;
                }
                d.Add(key, value);
            }else{
                d.Add(key, value);
            }
        }
    }

}
