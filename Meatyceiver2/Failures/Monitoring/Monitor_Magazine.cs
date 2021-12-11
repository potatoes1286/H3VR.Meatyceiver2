using FistVR;
using HarmonyLib;
using UnityEngine;

namespace Meatyceiver2.Failures.Monitoring
{
	public class Monitor_Magazine : MonoBehaviour
	{
		[HarmonyPatch(typeof(FVRFireArmMagazine), "Fire")]
		[HarmonyPrefix]
		static bool GeneralPatch_FailureToExtract(FVRFireArmMagazine __instance)
		{
			MCM.IncRoundsUsed(__instance);
			return true;
		}
	}
}