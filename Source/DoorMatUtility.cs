using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SyrDoorMats;
public static class DoorMatUtility
{
	/// <summary>
	/// preforms null checking and compares the intelligenceOrder of two TrainabilityDefs
	/// </summary>
	/// <returns>true if both are non-null and a's intelligenceOrder is greater than or equal to b's</returns>
	public static bool CompareTrainabilityDef(TrainabilityDef a, TrainabilityDef b)
	{
		return (a != null && b != null) && a.intelligenceOrder >= b.intelligenceOrder;
	}

	/// <summary>
	/// Logic checks for if a given animal pawn can us doormts
	/// </summary>
	public static bool AnimalCanUseDoormat(Pawn pawn)
	{

#if RELEASE_1_5
	return DoorMatsSettings.allowAnimals &&  pawn.RaceProps.Animal && pawn.Faction.IsPlayerSafe() && CompareTrainabilityDef(pawn.RaceProps?.trainability, DoorMatsSettings.RequiredIntelligence);
#else
		return DoorMatsSettings.allowAnimals &&  pawn.IsAnimal && pawn.Faction.IsPlayerSafe() && CompareTrainabilityDef(TrainableUtility.GetTrainability(pawn), DoorMatsSettings.RequiredIntelligence);
#endif
	}

	/// <summary>
	/// Logic checks if a given pawn can us doormts
	/// </summary>
	public static bool PawnCanUseDoormat(Pawn pawn)
	{
		return pawn != null &&
		  !(pawn.HostileTo(Faction.OfPlayer) || pawn.InAggroMentalState) && // Non-hostile paws
		  (!pawn.AnimalOrWildMan() || AnimalCanUseDoormat(pawn)) && //people and (if enabled) animals;
		  !(pawn.Drafted || pawn.health.hediffSet.BleedRateTotal > 0.01) && // That aren't drafted or substanitally bleeding
		  !(pawn.CurJob != null && (pawn.CurJobDef == JobDefOf.Flee || pawn.CurJobDef == JobDefOf.FleeAndCower || pawn.CurJobDef == JobDefOf.TendPatient || pawn.CurJobDef.driverClass == typeof(JobDriver_TakeToBed))); // and not otherwise doing something more important
		;
	}
}
