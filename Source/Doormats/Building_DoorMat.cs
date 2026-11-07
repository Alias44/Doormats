using RimWorld;
using Verse;
using System.Reflection;

namespace SyrDoormats;

/// <summary>
/// Doormat Thing code, modeled after a vanilla trap but always fires and won't cause a diplomatic incident when it does.  
/// </summary>
/// <seealso cref="RimWorld.Building_Trap"/>
public class Building_Doormat : Building, IPathFindCostProvider
{
	private List<Pawn> touchingPawns = new List<Pawn>();
	
	public static FieldInfo carriedFilthList =
		typeof(Pawn_FilthTracker).GetField("carriedFilth", BindingFlags.NonPublic | BindingFlags.Instance) ??
		throw new InvalidOperationException("Failed to find Pawn_FilthTracker.carriedFilth");

	public void Notify_PawnApproaching(Pawn pawn)
	{
		var carriedFilth = (List<Filth>)(carriedFilthList.GetValue(pawn.filth) ??
		    throw new NullReferenceException("Attempted to get value of pawn.filth and got null"));

		if (carriedFilth.NullOrEmpty()) return;

		foreach (var filth in carriedFilth)
		{
			var f = FilthMaker.TryMakeFilth(Position, Map, filth.def, filth.sources);
		}

		carriedFilthList.SetValue(pawn.filth, new List<Filth>());
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
		if (DoorMatUtility.PawnCanUseDoormat(p))
		{
			return DoorMatsSettings.slowdown;
		}

		return base.PathWalkCostFor(p);
	}

	/// <summary>
	/// Actions for the doormat to execute every game tick. Loosely based off of Building_Trap.Tick()
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
				if (thingList[i] is Pawn pawn && DoorMatUtility.PawnCanUseDoormat(pawn) && !touchingPawns.Contains(pawn))
				{
					touchingPawns.Add(pawn);
					Notify_PawnApproaching(pawn);
				}
			}

			// Prevents re-cleaning the same pawn multiple times as they move over the doormat 
			touchingPawns.RemoveAll(pawn2 => !pawn2.Spawned || pawn2.Flying || pawn2.Position != base.Position);
		}

		base.Tick();
	}
}