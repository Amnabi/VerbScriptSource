using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace VerbScript {

	public static class Recast{
		public static HashSet<Type> resolvedRedirect = new HashSet<Type>();
		public static Dictionary<Type, Type> destinationRedirect = new Dictionary<Type, Type>();

		public static Dictionary<Type, RecastDestinationHandler> recastDictionary = new Dictionary<Type, RecastDestinationHandler>();
		public static Dictionary<Type, HashSet<Type>> recastableCheckDictionary = new Dictionary<Type, HashSet<Type>>();
		public static void addRecastableEntry(Type destination, Type from){
			func9792F(destination, from);
			foreach(Type t in from.AllSubclasses()){
				func9792F(destination, t);
			}
		}
		public static void func9792F(Type destination, Type from){
			if(recastableCheckDictionary.TryGetValue(from, out HashSet<Type> tt)){
				tt.Add(destination);
				return;
			}
			HashSet<Type> hast = new HashSet<Type>();
			hast.Add(destination);
			recastableCheckDictionary.Add(from, hast);
		}
		public static bool recastableTo(this object a, Type recastTypeDestination){
			Type tCheck = a.GetType();
			if(recastTypeDestination.IsAssignableFrom(tCheck)){
				return true;
			}
			Type redir = null;
			destinationRedirect.TryGetValue(recastTypeDestination, out redir);
			recastTypeDestination = redir?? recastTypeDestination;//destinationRedirect[recastTypeDestination];
			if(tCheck != recastTypeDestination){
				if(recastableCheckDictionary.TryGetValue(tCheck, out HashSet<Type> tt)){
					return tt.Contains(recastTypeDestination);
				}
				return false;
			}
			return true;
		}

		public static void recastActor(Type A, RecastDestinationHandler rds){
			if(!resolvedRedirect.Contains(A)){
				resolvedRedirect.Add(A);
				destinationRedirect.Add(A, A);
				foreach(Type type in A.AllSubclasses()){
					destinationRedirect.Add(type, A);
				}
			}
			recastDictionary.Add(A, rds);
		}

		static Recast(){
			//Faction group
			addRecastableEntry(typeof(Faction), typeof(Map));
			addRecastableEntry(typeof(Faction), typeof(Thing));
			addRecastableEntry(typeof(Faction), typeof(Hediff));
			recastActor(typeof(Faction), new RDS_Faction());

			//Map group
			addRecastableEntry(typeof(Map), typeof(Thing));
			addRecastableEntry(typeof(Map), typeof(Hediff));
			recastActor(typeof(Map), new RDS_Map());

			//Thing group
			addRecastableEntry(typeof(Thing), typeof(LocalTargetInfo));
			addRecastableEntry(typeof(Thing), typeof(Hediff));
			recastActor(typeof(Thing), new RDS_Thing());
			
			//Vector3 group
			addRecastableEntry(typeof(Vector3), typeof(Hediff));
			addRecastableEntry(typeof(Vector3), typeof(Thing));
			addRecastableEntry(typeof(Vector3), typeof(LocalTargetInfo));
			addRecastableEntry(typeof(Vector3), typeof(IntVec3));
			recastActor(typeof(Vector3), new RDS_Vector3());
			
			addRecastableEntry(typeof(IntVec3), typeof(Hediff));
			addRecastableEntry(typeof(IntVec3), typeof(Thing));
			addRecastableEntry(typeof(IntVec3), typeof(LocalTargetInfo));
			addRecastableEntry(typeof(IntVec3), typeof(Vector3));
			recastActor(typeof(IntVec3), new RDS_IntVec3());
			
			addRecastableEntry(typeof(LocalTargetInfo), typeof(Hediff));
			addRecastableEntry(typeof(LocalTargetInfo), typeof(Thing));
			addRecastableEntry(typeof(LocalTargetInfo), typeof(Vector3));
			addRecastableEntry(typeof(LocalTargetInfo), typeof(IntVec3));
			recastActor(typeof(LocalTargetInfo), new RDS_LocalTargetInfo());
			//Primitive group
			addRecastableEntry(typeof(int), typeof(float));
			recastActor(typeof(int), new RDS_Int());
			
			addRecastableEntry(typeof(string), typeof(Vector3));
			addRecastableEntry(typeof(string), typeof(float));
			addRecastableEntry(typeof(string), typeof(Pawn));
			recastActor(typeof(string), new RDS_String());


		}
		public class RecastDestinationHandler{
			public virtual object recast(object val){
				return val;
			}
		}
		public class RDS_LocalTargetInfo : RecastDestinationHandler{
			public override object recast(object val){
				if(val is Hediff hed){
					return new LocalTargetInfo(hed.pawn);
				}
				if(val is Thing thing){
					return new LocalTargetInfo(thing);
				}
				if(val is Vector3 vec3){
					return new LocalTargetInfo(vec3.ToIntVec3());
				}
				if(val is IntVec3 ivec3){
					return new LocalTargetInfo(ivec3);
				}
				return base.recast(val);
			}
		}
		public class RDS_String : RecastDestinationHandler{
			public override object recast(object val){
				if(val is float floa){
					return floa.ToString();
				}
				if(val is Pawn pawn){
					return pawn.GetUniqueLoadID().ToString();
				}
				if(val is Vector3 vec3){
					return vec3.ToString();
				}
				return base.recast(val);
			}
		}
		public class RDS_Int : RecastDestinationHandler{
			public override object recast(object val){
				if(val is float floa){
					return (int)floa;
				}
				return base.recast(val);
			}
		}
		public class RDS_Faction : RecastDestinationHandler{
			public override object recast(object val){
				if(val is Thing thing){
					return thing.Faction;
				}
				if(val is Hediff hed){
					return hed.pawn.Faction;
				}
				if(val is Map map){
					return map.ParentFaction;
				}
				return base.recast(val);
			}
		}

		public class RDS_Map : RecastDestinationHandler{
			public override object recast(object val){
				if(val is Thing thing){
					return thing.MapHeld;
				}
				if(val is Hediff hed){
					return hed.pawn.MapHeld;
				}
				return base.recast(val);
			}
		}

		public class RDS_Thing : RecastDestinationHandler{
			public override object recast(object val){
				if(val is Hediff hed){
					return hed.pawn;
				}
				if(val is LocalTargetInfo lti){
					return lti.Thing;
				}
				return base.recast(val);
			}
		}
		//Vec3Group
		public class RDS_Vector3 : RecastDestinationHandler{
			public override object recast(object val){
				if(val is IntVec3 vec3){
					return vec3.ToVector3();
				}
				if(val is Thing thing){
					return thing.DrawPos;
				}
				if(val is Hediff hediff){
					return hediff.pawn.DrawPos;
				}
				if(val is LocalTargetInfo lti){
					return lti.CenterVector3;
				}
				return base.recast(val);
			}
		}
		public class RDS_IntVec3 : RecastDestinationHandler{
			public override object recast(object val){
				if(val is Vector3 hed){
					return hed.ToIntVec3();
				}
				if(val is Thing thing){
					return thing.Position;
				}
				if(val is Hediff hediff){
					return hediff.pawn.Position;
				}
				if(val is LocalTargetInfo lti){
					return lti.Cell;
				}
				return base.recast(val);
			}
		}

		/**public static T tryRecast<T>(this object val){
            if (val.recastableTo(typeof(T))) {
				return val.recast<T>();
			}
			return (T)null;
		}**/

		public static T recast<T>(this object val){
			if(val == null){
				return (T)val;
			}
			Type redirectOut = null;
			try{
				if(typeof(T).IsAssignableFrom(val.GetType())){
					return (T)val;
				}

				RecastDestinationHandler reh;
				destinationRedirect.TryGetValue(typeof(T), out redirectOut);
				redirectOut = redirectOut?? typeof(T);
				if(recastDictionary.TryGetValue(redirectOut, out reh)){
					return (T)reh.recast(val);
				}
				return (T)val;
			}catch{
				Log.Error("Failed to recast " + val + "(" + val.GetType() + ")" + " to " + typeof(T) + " /FinalType " + redirectOut);
			}
			throw new Exception();
		}
		public static object recast(this object val, Type T){
			if(val == null){
				return val;
			}
			Type redirectOut = null;
			try{
				if(T.IsAssignableFrom(val.GetType())){
					return val;
				}

				RecastDestinationHandler reh;
				destinationRedirect.TryGetValue(T, out redirectOut);
				redirectOut = redirectOut?? T;
				if(recastDictionary.TryGetValue(redirectOut, out reh)){
					return reh.recast(val);
				}
				return val;
			}catch{
				Log.Error("Failed to recast " + val + " to " + T + " /FinalType " + redirectOut);
			}
			throw new Exception();
		}
	}
}
