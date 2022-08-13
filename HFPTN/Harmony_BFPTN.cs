using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Reflection.Emit;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BFPTN
{
	public class BFPTNMod : Mod
	{
		public static BFPTNSettings settings;
		public BFPTNMod(ModContentPack content) : base(content)
		{
			settings = GetSettings<BFPTNSettings>();
		}
		public override void DoSettingsWindowContents(Rect inRect)
		{
			settings.DoWindowContents(inRect);
		}
		public override string SettingsCategory()
		{
			return "BFPTN".Translate();
		}
	}
	
	[StaticConstructorOnStartup]
    public static class Harmony_BFPTN{
        static Harmony_BFPTN(){
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("Amnabi.BFPTN");
            //harmony.PatchAll();
			foreach(BFPTNDef def in DefDatabase<BFPTNDef>.AllDefs){
				def.initializeOptimizations();
			}
			foreach(BFPTNDef def in DefDatabase<BFPTNDef>.AllDefs){
				def.initializeOptimizations2();
			}

        }

    }
}