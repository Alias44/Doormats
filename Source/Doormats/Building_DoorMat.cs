using RimWorld;
using Verse;
using System.Reflection;

namespace SyrDoormats;

public class Building_DoorMat : Building
public class Building_Doormat : Building
{
	public static FieldInfo carriedFilthList =
		typeof(Pawn_FilthTracker).GetField("carriedFilth", BindingFlags.NonPublic | BindingFlags.Instance) ??
		throw new InvalidOperationException("Failed to find Pawn_FilthTracker.carriedFilth");

	public void Notify_PawnApproaching(Pawn pawn)
	{
		var carriedFilth = (List<Filth>)(carriedFilthList.GetValue(pawn.filth) ??
		    throw new NullReferenceException("Attempted to get value of pawn.filth and got null"));

		if (carriedFilth.NullOrEmpty()) return;
		
		var filth = carriedFilth.RandomElement();
		FilthMaker.TryMakeFilth(Position, Map, filth.def, filth.sources);
		carriedFilthList.SetValue(pawn.filth, new List<Filth>());
	}

	public override void DrawGUIOverlay()
	{
		return;
	}
}