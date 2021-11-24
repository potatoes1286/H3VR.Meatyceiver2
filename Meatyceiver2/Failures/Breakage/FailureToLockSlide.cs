using FistVR;
using HarmonyLib;

namespace Meatyceiver2.Failures.Breakage
{
	public class FailureToLockSlide
	{
		[HarmonyPatch(typeof(Handgun), "EngageSlideRelease")] [HarmonyPrefix]
		static bool HandgunPatch_FailureToLockSlide(Handgun __instance)
		{
			if (!Meatyceiver.enableBrokenFirearmFailures.Value) return true;
			string failureName = "Failure to lock slide";
			float chance = Meatyceiver.FTLSlide.Value * Meatyceiver.generalMult.Value;
			if (Meatyceiver.CalcFail(chance, __instance))
				return false;
			return true;
		}
	}
}