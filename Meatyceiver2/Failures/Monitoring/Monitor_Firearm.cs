using FistVR;
using HarmonyLib;
using UnityEngine;

namespace Meatyceiver2.Failures.Monitoring
{
	public class Monitor_Firearm : MonoBehaviour
	{
		[HarmonyPatch(typeof(FVRFireArmChamber), "Fire")]
		[HarmonyPrefix]
		static bool GeneralPatch_FailureToExtract(FVRFireArmChamber __instance)
		{
			if(__instance.Firearm != null) MCM.IncRoundsUsed(__instance.Firearm);
			return true;
		}
	}
}