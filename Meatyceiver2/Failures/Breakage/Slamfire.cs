using HarmonyLib;
using FistVR;

namespace Meatyceiver2.Failures.Breakage
{
	public class Slamfire
	{
		[HarmonyPatch(typeof(HandgunSlide), "SlideEvent_ArriveAtFore")] [HarmonyPostfix]
		static void HandgunPatch_Slamfire(HandgunSlide __instance)
		{
			if (Meatyceiver.enableBrokenFirearmFailures.Value)
			{
				float chance = Meatyceiver.slamfireRate.Value * Meatyceiver.generalMult.Value;
				if (Meatyceiver.CalcFail(chance, __instance.Handgun)) {
					__instance.Handgun.DropHammer(false);
				}
			}
		}
		
		[HarmonyPatch(typeof(ClosedBolt), "BoltEvent_ArriveAtFore")] [HarmonyPostfix]
		static void ClosedBoltPatch_Slamfire(ClosedBolt __instance)
		{
			if (Meatyceiver.enableBrokenFirearmFailures.Value)
			{
				string failureName = "Slam fire";
				float chance = Meatyceiver.slamfireRate.Value * Meatyceiver.generalMult.Value;
				if (Meatyceiver.CalcFail(chance, __instance.Weapon))
				{
					__instance.Weapon.DropHammer();
				}
			}
		}
	}
}