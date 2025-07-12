using HarmonyLib;
using System;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;
using System.Collections.Generic;
using System.Diagnostics;


namespace SyrDoorMats
{
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		static HarmonyPatches()
		{
			var harmony = new Harmony("Syrchalis.Rimworld.Doormats");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			List<BackCompatibilityConverter> compatibilityConverters = 
				AccessTools.StaticFieldRefAccess<List<BackCompatibilityConverter>>(typeof(BackCompatibility), "conversionChain");

			compatibilityConverters.Add(new BackCompatibilityConverter_ColoredMats());
		}
	}

	[HarmonyPatch(typeof(Pawn_PathFollower), "SetupMoveIntoNextCell")]
	public static class SetupMoveIntoNextCellPatch
	{
		[HarmonyPostfix]
		public static void SetupMoveIntoNextCell_Postfix(Pawn_PathFollower __instance, Pawn ___pawn)
		{
			Building_DoorMat building_DoorMat = ___pawn.Map.thingGrid.ThingAt<Building_DoorMat>(__instance.nextCell);

			if (building_DoorMat != null && DoorMatUtility.PawnCanUseDoormat(___pawn))
			{
				building_DoorMat?.Notify_PawnApproaching(___pawn);
			}
		}
	}

	[HarmonyPatch(typeof(Pawn_PathFollower), "CostToMoveIntoCell", [typeof(Pawn), typeof(IntVec3)])]
	public static class CostToMoveIntoCellPatch
	{
		[HarmonyPostfix]
		public static void CostToMoveIntoCell_Postfix(ref float __result, Pawn pawn, IntVec3 c)
		{
			Building_DoorMat building_DoorMat = pawn.Map.thingGrid.ThingAt<Building_DoorMat>(c);

			if (DoorMatsSettings.slowdown > 0 && building_DoorMat != null && DoorMatUtility.PawnCanUseDoormat(pawn))
			{
				  __result += DoorMatsSettings.slowdown;
			}
		}
	}
}
