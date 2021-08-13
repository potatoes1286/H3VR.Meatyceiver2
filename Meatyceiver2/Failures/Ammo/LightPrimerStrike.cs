using HarmonyLib;
using FistVR;

namespace Meatyceiver2.Failures.Ammo
{
	public class LightPrimerStrike
	{
		public static bool isAmmoFailEnabled => Meatyceiver.enableAmmunitionFailures.Value;

		public static bool CalcLightPrimerStrikeFail()
		{
			if (!isAmmoFailEnabled) return true;
			//if it fails, don't run the routine that fires it
			if (Meatyceiver.CalcFail(Meatyceiver.LPSFailureRate.Value * Meatyceiver.generalMult.Value))
				return false;
			return true;
		}
		
		
		[HarmonyPatch(typeof(FVRFireArmChamber), "Fire")] [HarmonyPrefix]
		static bool DefaultPatch_LightPrimerStrike(ref bool __result, FVRFireArmChamber __instance, FVRFireArmRound ___m_round)
		{
			if (__instance.Firearm is Revolver || __instance.Firearm is RevolvingShotgun) return true;
			if (CalcLightPrimerStrikeFail())
			{
				return true;
			}
			__result = false;
			return false;
		}

		[HarmonyPatch(typeof(Revolver), "Fire")] [HarmonyPrefix]
		static bool RevolverPatch_LightPrimerStrike(Revolver __instance)
		{
			if (CalcLightPrimerStrikeFail())
				return true;
			return false;
		}

		[HarmonyPatch(typeof(RevolvingShotgun), "Fire")] [HarmonyPrefix]
		static bool RevolvingShotgunPatch_LightPrimerStrike(RevolvingShotgun __instance)
		{
			if (CalcLightPrimerStrikeFail())
				return true;
			return false;
		}
	}
}