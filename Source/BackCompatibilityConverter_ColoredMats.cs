using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

namespace SyrDoorMats
{
	public class BackCompatibilityConverter_ColoredMats : BackCompatibilityConverter
	{
		public readonly struct MatConversion
		{
			public ThingDef ThingDef { get; } = null;
			public ColorDef ColorDef { get; } = null;

			public MatConversion() { }

			public MatConversion(ThingDef thingDef)
			{
				ThingDef = thingDef;
			}

			public MatConversion(ThingDef thingDef, ColorDef colorDef)
			{
				ThingDef = thingDef;
				ColorDef = colorDef;
			}
		}

		Dictionary<string, MatConversion> defReplacements = [];
		Dictionary<int, ColorDef> existingMats = [];

		public override bool AppliesToVersion(int majorVer, int minorVer) => majorVer == 1 && minorVer <= 4;

		public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
		{
			if (defType == typeof(ThingDef) && defReplacements.ContainsKey(defName))
			{
				return defReplacements[defName].ThingDef.defName;
			}

			return null;
		}

		public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
		{
			if(baseType == typeof(Thing))
			{
				string def = node["def"].InnerText;
				int id = Thing.IDNumberFromThingID(node["id"].InnerText);

				if (defReplacements.ContainsKey(def) && !def.EndsWith("Leather"))
				{
					var c = defReplacements[def].ColorDef;

					existingMats.Add(id, c);
				}
			}

			return null;
		}

		public override void PostExposeData(object obj)
		{
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
				return;

			if (obj is Building_DoorMat mat && existingMats.ContainsKey(mat.thingIDNumber))
			{
				mat.SetStuffDirect(ThingDefOf.Cloth);
				mat.ChangePaint(existingMats[mat.thingIDNumber]);
			}

			return;
		}

		public override void PreLoadSavegame(string loadingVersion)
		{
			existingMats.Clear();

			ColorDef[] colors = [ColorDefOf.Structure_Red, ColorDefOf.Structure_Orange, ColorDefOf.Structure_Green, ColorDefOf.Structure_Teal, ColorDefOf.Structure_Blue];
			Dictionary<string, ThingDef> conversionBase = new()
			{
				{"DoorMat", DoorMatDefOf.Alias_DoorMatSmall},
				{"DoorMatMedium", DoorMatDefOf.Alias_DoorMatMedium},
				{"DoorMatBig", DoorMatDefOf.Alias_DoorMatLarge},
				{"DoorMatLinked", DoorMatDefOf.Alias_DoorMatLinked}
			};

			defReplacements = conversionBase
				.Join(colors, x => true, y => true, (kv, color) => new { Key = kv.Key + color.LabelCap.ToString(), Value = new MatConversion(kv.Value, color) })
				.ToDictionary(kv => kv.Key, kv => kv.Value);

			foreach (var kv in conversionBase)
			{
				defReplacements[kv.Key + "Leather"] = new MatConversion(kv.Value);
			}
		}

		public override void PostLoadSavegame(string loadingVersion)
		{
			defReplacements.Clear();
			existingMats.Clear();
		}
	}
}
