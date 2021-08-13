using HarmonyLib;
using FistVR;
using UnityEngine;

namespace Meatyceiver2.Failures
{
	public class OtherFailures
	{
		[HarmonyPatch(typeof(FVRFireArmChamber), "Awake")] [HarmonyPrefix]
		static bool RollingBlockPatch_ForceExtractable(FVRFireArmChamber __instance)
		{
			if(__instance.Firearm is RollingBlock)
			{
				__instance.IsManuallyExtractable = true;
			}
			return true;
		}

		[HarmonyPatch(typeof(FVRFireArmRound), "OnTriggerEnter")] [HarmonyPrefix]
		public static bool RoundPatch_InsertEmptyRound(FVRFireArmRound __instance, ref Collider collider)
		{
			if (__instance.isManuallyChamberable && __instance.HoveredOverChamber == null && __instance.m_hoverOverReloadTrigger == null && collider.gameObject.CompareTag("FVRFireArmChamber"))
			{
				FVRFireArmChamber component = collider.gameObject.GetComponent<FVRFireArmChamber>();
				if (component.RoundType == __instance.RoundType && component.IsManuallyChamberable && component.IsAccessible && !component.IsFull)
				{
					__instance.HoveredOverChamber = component;
				}
			}
			if (__instance.isMagazineLoadable && __instance.HoveredOverChamber == null && collider.gameObject.CompareTag("FVRFireArmMagazineReloadTrigger"))
			{
				FVRFireArmMagazineReloadTrigger component2 = collider.gameObject.GetComponent<FVRFireArmMagazineReloadTrigger>();
				if (component2.IsClipTrigger)
				{
					if (component2 != null && component2.Clip != null && component2.Clip.RoundType == __instance.RoundType && !component2.Clip.IsFull() && (component2.Clip.FireArm == null || component2.Clip.IsDropInLoadable))
					{
						__instance.m_hoverOverReloadTrigger = component2;
					}
				}
				else if (component2.IsSpeedloaderTrigger)
				{
					if (!component2.SpeedloaderChamber.IsLoaded)
					{
						__instance.m_hoverOverReloadTrigger = component2;
					}
				}
				else if (component2 != null && component2.Magazine != null && component2.Magazine.RoundType == __instance.RoundType && !component2.Magazine.IsFull() && (component2.Magazine.FireArm == null || component2.Magazine.IsDropInLoadable))
				{
					__instance.m_hoverOverReloadTrigger = component2;
				}
			}
			if (__instance.isPalmable && __instance.ProxyRounds.Count < __instance.MaxPalmedAmount && collider.gameObject.CompareTag("FVRFireArmRound"))
			{
				FVRFireArmRound component3 = collider.gameObject.GetComponent<FVRFireArmRound>();
				if (component3.RoundType == __instance.RoundType && component3.QuickbeltSlot == null)
				{
					__instance.HoveredOverRound = component3;
				}
			}
			return false;
		}
	}
}