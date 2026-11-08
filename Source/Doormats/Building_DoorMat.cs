using RimWorld;
using Verse;
using System.Reflection;
using HarmonyLib;

namespace SyrDoormats;

/// <summary>
/// Doormat Thing code, modeled after a vanilla trap but always fires and won't cause a diplomatic incident when it does.  
/// </summary>
/// <seealso cref="RimWorld.Building_Trap"/>
public class Building_Doormat : Building, IPathFindCostProvider
{
	private List<Pawn> touchingPawns = [];
	
	/// <summary>
	/// Logic checks for if a given animal pawn can us doormats
	/// </summary>
	public static bool AnimalCanUseDoormat(Pawn pawn)
	{

#if RELEASE_1_5
	return DoorMatsSettings.allowAnimals && pawn.RaceProps.Animal && pawn.Faction.IsPlayerSafe() && CompareTrainabilityDef(pawn.RaceProps?.trainability, DoorMatsSettings.RequiredIntelligence);
#else
		return DoorMatsSettings.allowAnimals && pawn.IsAnimal && pawn.Faction.IsPlayerSafe() && DoorMatUtility.CompareTrainabilityDef(TrainableUtility.GetTrainability(pawn), DoorMatsSettings.RequiredIntelligence);
#endif
	}
	
	public static HashSet<JobDef> priorityJobs = [JobDefOf.Flee, JobDefOf.FleeAndCower, JobDefOf.TendPatient];
	
	/// <summary>
	/// Logic checks if a given pawn can use doormats
	/// </summary>
	public static bool PawnCanUseDoormat(Pawn pawn)
	{
		// Reject hostile and flying pawns
		if (pawn.HostileTo(Faction.OfPlayer) || pawn.InAggroMentalState || pawn.Flying) return false;
		
		// Reject animals unless enabled
		if (pawn.AnimalOrWildMan() && !AnimalCanUseDoormat(pawn)) return false;
		
		// Skip if setting is enabled and they don't have any dirt
		if (DoorMatsSettings.slowdownIfDirty && pawn.filth.CarriedFilthListForReading.NullOrEmpty()) return false;
		
		// Reject drafted or substantially bleeding
		if (pawn.Drafted || pawn.health.hediffSet.BleedRateTotal > 0.01) return false;

		// null safety
		if (pawn.CurJob == null || pawn.CurJobDef == null) return false;
		
		// Reject when the current task is more important
		return !(priorityJobs.Contains(pawn.CurJobDef) || pawn.CurJobDef.driverClass == typeof(JobDriver_TakeToBed));
	}

	private static readonly FieldInfo carriedFilthList = AccessTools.Field(typeof(Pawn_FilthTracker), "carriedFilth");
	private static readonly MethodInfo drop = AccessTools.Method(typeof(Pawn_FilthTracker), "DropCarriedFilth");

	public void Notify_PawnApproaching(Pawn pawn)
	{
		var carriedFilth = (List<Filth>)(carriedFilthList.GetValue(pawn.filth) ??
		                                 throw new NullReferenceException("Attempted to get value of pawn.filth and got null"));

		foreach (var filth in carriedFilth.ToList())
		{
			for (var i = filth.thickness - 1; i >= 0; i--)
			{
				drop.Invoke(pawn.filth, [filth]);
			}
		}
		
		// In case terrain (like packed dirt) will not accept filth or the cell has reached max filth thickness, clear the list anyway  
		if (!carriedFilth.NullOrEmpty())
		{
			carriedFilthList.SetValue(pawn.filth, new List<Filth>());
		}
	}

	public override void DrawGUIOverlay()
	{
		return;
	}

	public CellRect GetOccupiedRect()
	{
		return this.OccupiedRect();
	}

	/// <summary>
	/// Pathing cost for moving over the doormat, setting this to 0 prevents pawns from avoiding doormats
	/// </summary>
	public ushort PathFindCostFor(Pawn pawn)
	{
		return 0;
	}

	/// <summary>
	/// Actual walk cost for moving over the doormat.
	/// note: def isEdifice must be true
	/// </summary>
	public override ushort PathWalkCostFor(Pawn p)
	{
		if (PawnCanUseDoormat(p))
		{
			return DoorMatsSettings.slowdown;
		}

		return base.PathWalkCostFor(p);
	}
	
	
	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref touchingPawns, "testees", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && touchingPawns.RemoveAll((Pawn x) => x == null) != 0)
		{
			Log.Error("Removed null pawns from touchingPawns.");
		}
	}

	/// <summary>
	/// Actions for the doormat to execute every game tick. Loosely based off of <see cref="Building_Trap.Tick()"/>
	/// note: def tickerType cannot be Never
	/// </summary>
	protected override void Tick()
	{
		if (base.Spawned)
		{
			List<Thing> thingList = base.Position.GetThingList(base.Map);

			// ReSharper disable once ForCanBeConvertedToForeach (changes to thingList mid-tick will result in an error if ForEach is used)  
			for (var i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is Pawn pawn && PawnCanUseDoormat(pawn) && !touchingPawns.Contains(pawn))
				{
					touchingPawns.Add(pawn);
					Notify_PawnApproaching(pawn);
				}
			}

			// Prevents re-cleaning the same pawn multiple times as they move over the doormat 
			touchingPawns.RemoveAll(pawn2 => pawn2 == null || !pawn2.Spawned || pawn2.Flying || pawn2.Position != base.Position);
		}

		base.Tick();
	}
}