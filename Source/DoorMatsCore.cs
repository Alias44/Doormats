using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SyrDoorMats
{
	public class DoorMatsCore : Mod
	{
		public static DoorMatsSettings settings;

		public DoorMatsCore(ModContentPack content) : base(content)
		{
			settings = GetSettings<DoorMatsSettings>();
		}

		public override string SettingsCategory() => "SyrDoorMatsSettingsCategory".Translate();

		public override void DoSettingsWindowContents(Rect inRect)
		{
			checked
			{
				Listing_Standard listing = new();
				listing.Begin(inRect);
				listing.Label("SyrDoorMatsSlowdown".Translate() + ": " + DoorMatsSettings.slowdown, -1, tooltip: "SyrDoorMatsSlowdownToolTip".Translate());
				listing.Gap(6f);
				DoorMatsSettings.slowdown = (int)listing.Slider(GenMath.RoundTo(DoorMatsSettings.slowdown, 5), 0, 100);

				listing.Gap();
				listing.CheckboxLabeled("SyrDoorMatsAnimalsAllow".Translate(), ref DoorMatsSettings.allowAnimals, "SyrDoorMatsAnimalsAllowTip".Translate());

				// Animals
				if (DoorMatsSettings.allowAnimals && listing.ButtonTextLabeled("SyrDoorMatsAnimalsMinIntelligence".Translate(), DoorMatsSettings.RequiredIntelligenceDisplay(), tooltip: "SyrDoorMatsAnimalsIntelligenceToolTip".Translate()))
				{
					List<FloatMenuOption> options = [new FloatMenuOption("Disabled".Translate(), () => DoorMatsSettings.RequiredIntelligence = null)];

					foreach (var def in DefDatabase<TrainabilityDef>.AllDefs.OrderBy(def => def.intelligenceOrder).Reverse())
					{
						string label = def.LabelCap;

						if(def.Equals(TrainabilityDefOf.None))
						{
							label += "SyrDoorMatsAnimalsAll".Translate();
						}

						options.Add(new FloatMenuOption(label, () => DoorMatsSettings.RequiredIntelligence = def));
					}

					Find.WindowStack.Add(new FloatMenu(options));
				}

				listing.Gap(24f);
				if (listing.ButtonText("RestoreToDefaultSettings".Translate(), "RestoreToDefaultSettingsLabel".Translate()))
				{
					DoorMatsSettings.ResetToDefault();
				}

				listing.End();
				settings.Write();
			}
		}

		public override void WriteSettings()
		{
			base.WriteSettings();
		}
	}

	public class DoorMatsSettings : ModSettings
	{
		public static int slowdown = 40;
		public static bool allowAnimals = false;
		private static string defString;
        private static TrainabilityDef requiredIntelligence = null;

		public static TrainabilityDef RequiredIntelligence
		{
			get
			{
				if (defString != null)
				{
					requiredIntelligence = DefDatabase<TrainabilityDef>.GetNamedSilentFail(defString);
				}

				return requiredIntelligence;
			}

			set
			{
				requiredIntelligence = value;
				defString = requiredIntelligence?.defName;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref slowdown, "SyrDoorMatsSlowdown", 40, true);
			Scribe_Values.Look(ref defString, "requiredIntelligence");

			if(Scribe.mode == LoadSaveMode.LoadingVars)
			{
				allowAnimals = !defString.NullOrEmpty();
			}
		}

		public static void ResetToDefault()
		{
			slowdown = 40;
			allowAnimals = false;
			RequiredIntelligence = null;
		}

		public static string RequiredIntelligenceDisplay()
		{
			return RequiredIntelligence?.LabelCap ?? "Disabled".Translate();
		}
	}
}
