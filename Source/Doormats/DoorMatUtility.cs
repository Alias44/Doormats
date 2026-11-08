using Verse;

namespace SyrDoormats;
public static class DoorMatUtility
{
	/// <summary>
	/// preforms null checking and compares the intelligenceOrder of two TrainabilityDefs
	/// </summary>
	/// <returns>true if both are non-null and a's intelligenceOrder is greater than or equal to b's</returns>
	public static bool CompareTrainabilityDef(TrainabilityDef? a, TrainabilityDef? b)
	{
		return (a != null && b != null) && a.intelligenceOrder >= b.intelligenceOrder;
	}
}
