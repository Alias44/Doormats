using RimWorld;
using System.Xml;
using Verse;

namespace SyrDoormats;

public class BackCompatibilityConverter_ColoredMats : BackCompatibilityConverter
{
	public readonly struct MatConversion
	{
		public ThingDef ThingDef { get; }
		public ColorDef? ColorDef { get; }


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

	public override string? BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode? node = null)
	{
		if (defType == typeof(ThingDef) && defReplacements.TryGetValue(defName, out var replacement))
		{
			return replacement.ThingDef.defName;
		}

		return null;
	}

	public override Type? GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
	{
		if (baseType != typeof(Thing)) return null;
		
		// Verse.Thing.ExposeData() guarantees that a saved Thing will have a def attached 
		var def = node["def"]!.InnerText;

		if (!defReplacements.TryGetValue(def, out var replacement) || def.EndsWith("Leather")) return null;
		
		var c = replacement.ColorDef ?? ColorDefOf.Structure_Blue;
		var id = Thing.IDNumberFromThingID(node["id"]!.InnerText);

		existingMats.Add(id, c);

		return null;
	}

	public override void PostExposeData(object obj)
	{
		if (Scribe.mode != LoadSaveMode.PostLoadInit) return;

		if (obj is not Building_Doormat mat || !existingMats.TryGetValue(mat.thingIDNumber, out var color)) return;
		
		mat.SetStuffDirect(ThingDefOf.Cloth);
		mat.ChangePaint(color);

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
			.Join(colors, _ => true, _ => true, (kv, color) => new { Key = kv.Key + color.LabelCap.ToString(), Value = new MatConversion(kv.Value, color) })
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