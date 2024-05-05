using HarmonyLib;
using System;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;
using System.Collections.Generic;


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
			if (___pawn != null)
			{
				Building_DoorMat building_DoorMat = ___pawn.Map.thingGrid.ThingAt<Building_DoorMat>(__instance.nextCell);
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
			if (DoorMatsSettings.slowdown > 0 && pawn != null && (pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony))
			{
				if (pawn.Drafted || pawn.health.hediffSet.BleedRateTotal > 0.01)
				{
					return;
				}
				if (pawn.CurJob != null && (pawn.CurJobDef == JobDefOf.Flee || pawn.CurJobDef == JobDefOf.FleeAndCower || pawn.CurJobDef == JobDefOf.TendPatient || pawn.CurJobDef.driverClass == typeof(JobDriver_TakeToBed)))
				{
					return;
				}
				Building_DoorMat building_DoorMat = pawn.Map.thingGrid.ThingAt<Building_DoorMat>(c);
				if (building_DoorMat != null)
				{
					__result += DoorMatsSettings.slowdown;
				}
			}
		}
	}
}
