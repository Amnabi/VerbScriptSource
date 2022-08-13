using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace BFPTN {
    
	public class BFPTNSettings : ModSettings
	{
		public static int ticksPerCheck = 200;
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref ticksPerCheck, "ticksPerCheck");
		}

		public void DoWindowContents(Rect inRect)
		{
			float previous;
			Rect rect;
			var map = Current.Game?.CurrentMap;
			const float buttonWidth = 80f;
			const float buttonSpace = 4f;
			Listing_Standard list = new Listing_Standard { ColumnWidth = (inRect.width - 34f)};
			list.Begin(inRect);
			list.Gap(16f);
			list.Label("BFPTN.TicksPerCheck".Translate() + " " + ticksPerCheck);
			ticksPerCheck = (int)list.Slider(ticksPerCheck, 200, 180000);
			list.End();
		}
	}
}
