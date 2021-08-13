using FistVR;
using HarmonyLib;

namespace Meatyceiver2.Failures.Breakage
{
	public class FailureToLockSlide
	{
		[HarmonyPatch(typeof(Handgun), "EngageSlideRelease")] [HarmonyPrefix]
		static bool HandgunPatch_FailureToLockSlide()
		{
			if (!Meatyceiver.enableBrokenFirearmFailures.Value) return true;
			string failureName = "Failure to lock slide";
			float chance = Meatyceiver.FTLSlide.Value * Meatyceiver.generalMult.Value;
			if (Meatyceiver.CalcFail(chance))
				return false;
			return true;
		}
	}
}