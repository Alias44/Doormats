using RimWorld;
using Verse;

// These can be null if accessed before the game assigns them, but realistically that's pretty much just during early mod initalization.
#pragma warning disable CS8618
namespace SyrDoormats;

[DefOf]
public static class DoorMatDefOf
{
	public static ThingDef Alias_DoorMatSmall;
	public static ThingDef Alias_DoorMatMedium;
	public static ThingDef Alias_DoorMatLarge;
	public static ThingDef Alias_DoorMatLinked;
}