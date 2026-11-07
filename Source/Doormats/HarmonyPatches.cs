using HarmonyLib;
using System.Reflection;
using Verse;
using Verse.AI;


namespace SyrDoormats;

[StaticConstructorOnStartup]
public static class HarmonyPatches
{
	static HarmonyPatches()
	{
		var harmony = new Harmony("Alias.Rimworld.Doormats");
		harmony.PatchAll(Assembly.GetExecutingAssembly());

		List<BackCompatibilityConverter> compatibilityConverters = 
			AccessTools.StaticFieldRefAccess<List<BackCompatibilityConverter>>(typeof(BackCompatibility), "conversionChain");

		compatibilityConverters.Add(new BackCompatibilityConverter_ColoredMats());
	}
}