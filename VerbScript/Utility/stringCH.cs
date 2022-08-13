using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace VerbScript {
    public static class stringCHUtility{
        public static stringCH CH(this string str){
            return new stringCH(str);
        }
    }

    public struct stringCH : IExposable{
        public string text;
        public int hash;
        public stringCH(string str){
            text = str;
            hash = str.GetHashCode();
            if(str == null){
                Log.Error("Null string!");
            }
        }

        public override string ToString() {
            return text;
        }
        public override int GetHashCode(){
            /**if(hash == 0){
                hash = text.GetHashCode();
            }**/
            return hash;
        }
        public static bool operator ==(stringCH a, stringCH b){
            return a.Equals(b);
        }
        public static bool operator !=(stringCH a, stringCH b){
            return !a.Equals(b);
        }
        public override bool Equals(object obj) {
            if(obj is stringCH ch){
                return ch.hash == hash && text == ch.text;
            }
            return false;
        }
        public void setString(string str){
            text = str;
            hash = str.GetHashCode();
        }
        public void ExposeData(){
            Scribe_Values.Look<string>(ref text, "CAA_stringText");
            if(Scribe.mode == LoadSaveMode.LoadingVars){
                hash = text.GetHashCode();
            }
        }
    }
}
