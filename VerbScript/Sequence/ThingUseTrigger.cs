using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace VerbScript {
    public class ThingUseTrigger {
        public static Dictionary<string, ThingDef> stringToThingDefActivator = new Dictionary<string, ThingDef>();
        public static Func<string, CompProperties, ThingDef> defaultGetter = delegate(string str, CompProperties compProps){
			ThingDef tdi = new ThingDef {
				generated = true,
				defName = str,
				label = "ProcGenItem_YOUSHOULDNOTSEETHIS",
				description = "YOU SHOULD NOT SEE THIS",
				category = ThingCategory.Item,
				selectable = true,
				thingClass = typeof(ThingWithComps),
				comps = new List<CompProperties>
				{
					compProps,
					new CompProperties_UseEffect
					{
						compClass = typeof(CompUseEffect_DestroySelf)
					},
					new CompProperties_Forbiddable()
				},
				graphicData = new GraphicData
				{
					texPath = "Things/Item/Special/MechSerumNeurotrainer",
					graphicClass = typeof(Graphic_Single)
				},
				drawGUIOverlay = false,
				statBases = new List<StatModifier>
				{
				},
				techLevel = TechLevel.Ultra,
				altitudeLayer = AltitudeLayer.Item,
				alwaysHaulable = false,
				rotatable = false,
				pathCost = DefGenerator.StandardItemPathCost,
				tradeTags = new List<string>{
				},
				stackLimit = 1,
				tradeNeverStack = true,
				forceDebugSpawnable = true
			};
			tdi.PostLoad();
			DefDatabase<ThingDef>.Add(tdi);
			return tdi;
        };

    }
}
