using RimWorld;

// These can be null if accessed before the game assigns them, but realistically that's pretty much just during early mod initalization. 
#pragma warning disable CS8618
namespace SyrDoormats;

[DefOf]
public static class ColorDefOf
{
	public static ColorDef Structure_Red;
	public static ColorDef Structure_Orange;
	public static ColorDef Structure_Green;
	public static ColorDef Structure_Teal;
	public static ColorDef Structure_Blue;
}