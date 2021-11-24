﻿using FistVR;
using HarmonyLib;

namespace Meatyceiver2.Failures.Breakage
{
	public class HammerFollow
	{
		[HarmonyPatch(typeof(ClosedBoltWeapon), "CockHammer")] [HarmonyPrefix]
		static bool ClosedBoltPatch_HammerFollow(ClosedBoltWeapon __instance)
		{
			if (!Meatyceiver.enableBrokenFirearmFailures.Value) return true;
			float chance = Meatyceiver.HFRate.Value * Meatyceiver.generalMult.Value;
			if (Meatyceiver.CalcFail(chance, __instance))
				return false;
			return true;
		}

		[HarmonyPatch(typeof(Handgun), "CockHammer")]
		[HarmonyPrefix]
		private static bool HFHandgun(bool isManual, Handgun __instance)
		{
			if (!Meatyceiver.enableBrokenFirearmFailures.Value) return true;
			float chance = Meatyceiver.HFRate.Value * Meatyceiver.generalMult.Value;
			if (Meatyceiver.CalcFail(chance, __instance))
				return false;
			return true;
		}
	}
}