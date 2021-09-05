using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace GreenThumbRevived
{
	[StaticConstructorOnStartup]
	public static class GreenThumbRevived
	{
		static GreenThumbRevived() 
		{
			var harmony = new Harmony("com.GreenThumbRevived.patch");
			harmony.PatchAll();
			GreenThumbRevived_Settings.ApplySettings();
		}
	}

	[HarmonyPatch(typeof(CompPlantable), "DoPlant")]
	public static class CompPlantable_DoPlant_Patch
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn planter, IntVec3 cell, Map map)
		{
			Log.Message("planted thing");
			GreenThumbRevived_Mod.GiveGreenThumbThought(planter);
		}
	}

	public class GreenThumbRevived_Mod : Mod
    {
		public GreenThumbRevived_Mod(ModContentPack content) : base(content)
		{
			GreenThumbRevived_Mod.settings = base.GetSettings<GreenThumbRevived_Settings>();
		}
		public override string SettingsCategory()
		{
			return "GreenThumbRevived";
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			GreenThumbRevived_Settings.DoWindowContents(inRect);
		}

		public static void GiveGreenThumbThought(Pawn planter)
        {
			if (planter.story.traits.HasTrait(TraitDefOf.GreenThumbRevived2))
            {
				planter.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.GreenThumbRevived);
			}
        }

		public static GreenThumbRevived_Settings settings;
	}

	public class GreenThumbRevived_Settings : ModSettings
    {
		public static int maxStacks;
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref maxStacks, "maxStacks", 15, true);
		}
		public static void DoWindowContents(Rect inRect)
		{
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(inRect);
			listing_Standard.Label("Maximum Green Thumb Bonus: " + maxStacks.ToString(), -1f, null);
			maxStacks = Mathf.RoundToInt(listing_Standard.Slider(maxStacks, 0f, 15f));
			listing_Standard.End();

			Rect rect = inRect.BottomPart(0.1f).LeftPart(0.1f);
			bool flag = Widgets.ButtonText(rect, "Apply Settings", true, true, true);
			bool flag2 = flag;
			if (flag2)
			{
				GreenThumbRevived_Settings.ApplySettings();
			}
		}
		public static void ApplySettings()
        {
			ThoughtDefOf.GreenThumbRevived.stackLimit = maxStacks;
        }
	}

	[DefOf]
	public static class ThoughtDefOf
	{
		public static ThoughtDef GreenThumbRevived;
	}

	[DefOf]
	public static class TraitDefOf
	{
		public static TraitDef GreenThumbRevived2;
	}

	public class ThoughtWorker_GreenThumbRevived : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			int countplanted = CountPlanted(HistoryEventDefOf.SowedPlant);
			return ThoughtState.ActiveAtStage(Math.Min(countplanted/3, 4));
			
		}
		
		public int CountPlanted(HistoryEventDef def)
        {
			return Find.HistoryEventsManager.GetRecentCountWithinTicks(def, 60000);
        }
	}
}