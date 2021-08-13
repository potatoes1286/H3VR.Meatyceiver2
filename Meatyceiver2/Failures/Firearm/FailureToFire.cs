using HarmonyLib;
using FistVR;

namespace Meatyceiver2.Failures.Firearm
{
	public class FailureToFire
	{
		public static bool isFirearmFailEnabled => Meatyceiver.enableFirearmFailures.Value;
		
		[HarmonyPatch(typeof(ClosedBoltWeapon), "BeginChamberingRound")]
		[HarmonyPatch(typeof(OpenBoltReceiver), "BeginChamberingRound")]
		[HarmonyPatch(typeof(Handgun), "ExtractRound")]
		[HarmonyPrefix]
		static bool DefaultPatch_FailureToFeed(FVRFireArm __instance)
		{
			float failureinc = 0;
			if (!isFirearmFailEnabled) { return true; }
			if (__instance.Magazine != null && Meatyceiver.enableMagUnreliability.Value)
			{
				if (!__instance.Magazine.IsBeltBox)
				{
					//if the box mag's cap is over the cap that would begin failures
					if (__instance.Magazine.m_capacity > Meatyceiver.minRoundCount.Value) {
						//diff between mag cap and min round cap and multuiply by failure inc per round
						float baseFailureInc =
							(__instance.Magazine.m_capacity - Meatyceiver.minRoundCount.Value)
							* Meatyceiver.failureIncPerRound.Value;
						//failure inc = the failure inc * general mult - 1 * mag unreliability (?)
						failureinc = baseFailureInc +
						     (baseFailureInc 
							* Meatyceiver.generalMult.Value - 1
							* Meatyceiver.magUnreliabilityGenMultAffect.Value);
					}
				}
			}
			//then calculate the rate, + what we just got before
			float chance = Meatyceiver.HFRate.Value
			               * Meatyceiver.generalMult.Value
			               + failureinc;
			//throw that meat pile n calc
			if (Meatyceiver.calcFail(chance))
				return false;
			return true;
		}
	}
}