using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using FistVR;
using UnityEngine;
using BepInEx.Harmony;
using BepInEx;
using BepInEx.Configuration;

namespace Meatyceiver2
{
	[BepInPlugin("dll.potatoes.meatyceiver2", "Meatyceiver2", "0.2.4")]
	public class Meatyceiver : BaseUnityPlugin
	{
		private static ConfigEntry<bool> enableFirearmFailures;
		private static ConfigEntry<bool> enableAmmunitionFailures;
		private static ConfigEntry<bool> enableBrokenFirearmFailures;
		private static ConfigEntry<bool> enableSecondaryMultipliers;

		private static ConfigEntry<bool> enableConsoleDebugging;

		private static ConfigEntry<float> generalMult;
		private static ConfigEntry<float> pistolMult;

		private static ConfigEntry<float> failureIncPerRound;
		private static ConfigEntry<int> minRoundCount;


		private static ConfigEntry<float> lightPrimerStrikeFailureRate;
		private static ConfigEntry<float> HangFireRate;

		private static ConfigEntry<float> failureToFeedRate;
		private static ConfigEntry<float> FailureToExtractRate;
		private static ConfigEntry<float> DoubleFeedRate;
		private static ConfigEntry<float> StovepipeRate;

		private static ConfigEntry<float> HammerFollowRate;
		private static ConfigEntry<float> failureToLockSlide;
		private static ConfigEntry<float> SlamfireRate;

		private static ConfigEntry<float> BespokeFailureBreakActionShotgunFTE;

		public static float prevSlideZLock = -999f;

		public static System.Random rnd;

		void Awake()
		{
			UnityEngine.Debug.Log("Meatyceiver2 here!");
			enableAmmunitionFailures = Config.Bind("_General Settings", "Enable Ammunition Failures", true, "Enables ammunition related failures.");
			enableFirearmFailures = Config.Bind("_General Settings", "Enable Firearm Failures", true, "Enables firearm related failures.");
			enableBrokenFirearmFailures = Config.Bind("_General Settings", "Enable Broken Firearm Failures", true, "Enables failures related to permanent firearm damage.");
			enableSecondaryMultipliers = Config.Bind("_General Settings", "Enable Secondary Failure Multipliers", true, "Enables secondary jam chance multipliers.");
			enableConsoleDebugging = Config.Bind("_General Settings", "Enable Console Debugging", false, "Exports values and failures to console.");

			generalMult = Config.Bind("_Multipliers", "Failure Chance Multiplier", 1f, "default at 1x is 1%, so this is a more 'pick failure percentage chance'.");
			failureIncPerRound = Config.Bind("_Multipliers", "Additional Failure Chance Per Round", 0.01f, "Every round in a mag past Minimum Mag Count increases FTF failure percent chance this much. Secondary failure multiplier.");
			minRoundCount = Config.Bind("_Multipliers", "Minimum Mag Count", 15, "Max mag round counts above this incurs higher unreliability.");

			//			pistolMult = Config.Bind("_Multipliers", "Pistol Failure Multiplier", 1f, "Pistols are higher than others because they are semi.");

			lightPrimerStrikeFailureRate = Config.Bind("Failures - Ammo", "Light Primer Strike Failure Rate", 0.25f, "Valid numbers are 0-100");
			HangFireRate = Config.Bind("Failures - Ammo", "Hang Fire Rate", 0.1f, "Valid numbers are 0-100");

			failureToFeedRate = Config.Bind("Failures - Firearm", "Failure to Feed Rate", 0.25f, "Valid numbers are 0-100");
			FailureToExtractRate = Config.Bind("Failures - Firearm", "Failure to Eject Rate", 0.15f, "Valid numbers are 0-100");
			DoubleFeedRate = Config.Bind("Failures - Firearm", "Double Feed Rate", 0.15f, "Valid numbers are 0-100");
			StovepipeRate = Config.Bind("Failures - Firearm", "Stovepipe Rate", 0.1f, "Valid numbers are 0-100");

			HammerFollowRate = Config.Bind("Failures - Broken Firearm", "Hammer Follow Rate", 0.05f, "Valid numbers are 0-100");
			failureToLockSlide = Config.Bind("Failures - Broken Firearm", "Failure to Lock Slide Rate", 0.3f, "Valid numbers are 0-100");
			SlamfireRate = Config.Bind("Failures - Broken Firearm", "Slam Fire Rate", 0.05f, "Valid numbers are 0-100");

			BespokeFailureBreakActionShotgunFTE = Config.Bind("Failures - Bespoke", "Break Action Failure To Eject", 1.5f, "Valid numbers are 0-100");
			Harmony.CreateAndPatchAll(typeof(Meatyceiver));
			rnd = new System.Random();
		}

		//BEGIN AMMO FAILURES

		[HarmonyPatch(typeof(FVRFireArmChamber), "Fire")]
		[HarmonyPrefix]
		static bool LightPrimerStrikePatch(ref bool __result, FVRFireArmChamber __instance, FVRFireArmRound ___m_round)
		{
			if (!enableAmmunitionFailures.Value) { return true; }
			if (__instance.Firearm is Revolver || __instance.Firearm is RevolvingShotgun) { return true; }
			var rand = (float)rnd.Next(0, 10001) / 100;
			if (enableConsoleDebugging.Value) { Debug.Log("LPS RNG: " + rand + " to " + lightPrimerStrikeFailureRate.Value * generalMult.Value); }
			if (rand >= lightPrimerStrikeFailureRate.Value * generalMult.Value)
			{
				if (__instance.IsFull && ___m_round != null && !__instance.IsSpent)
				{
					__instance.IsSpent = true;
					__instance.UpdateProxyDisplay();
					__result = true;
					return false;
				}
			}
			else
			{
				if (enableConsoleDebugging.Value) { Debug.Log("Light primer strike!"); };
			}
			__result = false;
			return false;
		}

		[HarmonyPatch(typeof(Revolver), "Fire")]
		[HarmonyPrefix]
		static bool LightPrimerStrikeRevolverPatch(Revolver __instance)
		{
			if (!enableAmmunitionFailures.Value) { return true; }
			var rand = (float)rnd.Next(0, 10001) / 100;
			if (enableConsoleDebugging.Value) { Debug.Log("LPS RNG: " + rand + " to " + lightPrimerStrikeFailureRate.Value * generalMult.Value); }
			if (rand <= lightPrimerStrikeFailureRate.Value * generalMult.Value)
			{
				if (enableConsoleDebugging.Value) { Debug.Log("Light primer strike!"); };
				__instance.Chambers[__instance.CurChamber].IsSpent = false;
				__instance.Chambers[__instance.CurChamber].UpdateProxyDisplay();
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(RevolvingShotgun), "Fire")]
		[HarmonyPrefix]
		static bool LightPrimerStrikeRevolvingShotgunPatch(RevolvingShotgun __instance)
		{
			if (!enableAmmunitionFailures.Value) { return true; }
			var rand = (float)rnd.Next(0, 10001) / 100;
			if (enableConsoleDebugging.Value) { Debug.Log("LPS RNG: " + rand + " to " + lightPrimerStrikeFailureRate.Value * generalMult.Value); }
			if (rand <= lightPrimerStrikeFailureRate.Value * generalMult.Value)
			{
				if (enableConsoleDebugging.Value) { Debug.Log("Light primer strike!"); };
				__instance.Chambers[__instance.CurChamber].IsSpent = false;
				__instance.Chambers[__instance.CurChamber].UpdateProxyDisplay();
				return false;
			}
			return true;
		}


		//BEGIN FIREARM FAILURES

		[HarmonyPatch(typeof(ClosedBoltWeapon), "BeginChamberingRound")]
		[HarmonyPatch(typeof(OpenBoltReceiver), "BeginChamberingRound")]
		[HarmonyPatch(typeof(Handgun), "ExtractRound")]
		[HarmonyPrefix]
		static bool FTFPatch(FVRFireArm __instance)
		{
			float failureinc = 0;
			if (!enableFirearmFailures.Value) { return true; }
			var rand = (float)rnd.Next(0, 10001) / 100;
			if (__instance.Magazine != null && enableSecondaryMultipliers.Value)
			{
				if (!__instance.Magazine.IsBeltBox)
				{
					failureinc = (float)(__instance.Magazine.m_capacity - minRoundCount.Value) * failureIncPerRound.Value;
				}
			}
			if (enableConsoleDebugging.Value) { Debug.Log("FTF RNG: " + rand + " to " + (HammerFollowRate.Value + failureinc) * generalMult.Value); };
			if (rand <= (failureToFeedRate.Value + failureinc) * generalMult.Value)
			{
				if (enableConsoleDebugging.Value) { Debug.Log("Failure to feed!"); };
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(BreakActionWeapon), "PopOutRound")]
		[HarmonyPrefix]
		static bool FailtoPopEmptyBreakActionPatch(BreakActionWeapon __instance, FVRFireArm chamber)
		{
			if (!enableFirearmFailures.Value) { return true; }
			if (chamber.RotationInterpSpeed == 2) { return false; };
			var rand = (float)rnd.Next(0, 10001) / 100;
			if (enableConsoleDebugging.Value) { Debug.Log("FTE RNG: " + rand + " to " + BespokeFailureBreakActionShotgunFTE.Value * generalMult.Value); }
			if (rand <= BespokeFailureBreakActionShotgunFTE.Value * generalMult.Value)
			{
				if (enableConsoleDebugging.Value) { Debug.Log("Failure to eject!"); }
				chamber.RotationInterpSpeed = 2;
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(FVRFireArmChamber), "BeginInteraction")]
		[HarmonyPostfix]
		static void fixBreakActionFTEPatch(FVRFireArmChamber __instance)
		{
			__instance.RotationInterpSpeed = 1;
		}

		/*		[HarmonyPatch(typeof(Handgun), "CockHammer")]
				[HarmonyPrefix]
				static bool HammerFollowPatch(bool ___isManual)
				{
					var rand = (float)rnd.Next(0, 10001) / 100;
					Debug.Log("Random number generated for HammerFollow: " + rand);
					if (rand <= HammerFollowRate.Value && !___isManual)
					{
						Debug.Log("Hammer follow!");
						return false;
					}
					return true;
				}*/
		[HarmonyPatch(typeof(ClosedBolt), "ImpartFiringImpulse")]
		[HarmonyPatch(typeof(HandgunSlide), "ImpartFiringImpulse")]
		[HarmonyPatch(typeof(OpenBoltReceiverBolt), "ImpartFiringImpulse")]
		[HarmonyPrefix]
		static bool FTEPatch(FVRInteractiveObject __instance)
		{
			if (__instance is BoltActionRifle) { return false; }
			if (__instance is LeverActionFirearm) { return false; }
			if (!enableFirearmFailures.Value) { return true; }
			var rand = (float)rnd.Next(0, 10001) / 100;
			if (enableConsoleDebugging.Value) { Debug.Log("Stovepipe RNG: " + rand + " to " + StovepipeRate.Value * generalMult.Value); }
			if (rand >= 100 - StovepipeRate.Value * generalMult.Value)
			{
				if (enableConsoleDebugging.Value) { Debug.Log("Stovepipe!"); }
				__instance.RotationInterpSpeed = 2;
				return false;
			}
			if (enableConsoleDebugging.Value) { Debug.Log("FTE RNG: " + rand + " to " + FailureToExtractRate.Value * generalMult.Value); }
			if (rand <= FailureToExtractRate.Value * generalMult.Value)
			{
				if (enableConsoleDebugging.Value) { Debug.Log("Failure to eject!"); }
				return false;
			}
			return true;
		}

		//BEGIN BROKEN FIREARM FAILURES

		/*		[HarmonyPatch(typeof(HandgunSlide), "UpdateSlide")]
				[HarmonyPrefix]
				static bool StovepipeHandgunSlidePatch(
					HandgunSlide __instance,
					float ___m_slideZ_forward,
					float ___m_slideZ_rear,
					float ___m_slideZ_lock,
					float ___m_slideZ_current)
				{
					if (prevSlideZLock == -999f)
					{
						prevSlideZLock = ___m_slideZ_lock;
					}

					if (__instance.RotationInterpSpeed == 2)
					{
						if (__instance.CurPos == HandgunSlide.SlidePos.Rear)
						{
							if (enableConsoleDebugging.Value) { Debug.Log("Stovepipe cleared!"); }
							__instance.RotationInterpSpeed = 1;
							___m_slideZ_lock = prevSlideZLock;
						}
						else
						{
							var m_slideStovePipe = ___m_slideZ_forward - (___m_slideZ_forward - ___m_slideZ_rear) / 2;
							if (___m_slideZ_current > m_slideStovePipe)
							{
								___m_slideZ_current = m_slideStovePipe;
							}
		//				Debug.Log("m_slideStovePipe: " + m_slideStovePipe);
						}

					}
					return true;
				}
		/*		[HarmonyPatch(typeof(Handgun), "IsSlideCatchEngaged")]
				[HarmonyPrefix]
				static bool StovepipeHandgunPatch(Handgun __instance, ref bool __result)
				{
					if (__instance.Slide.RotationInterpSpeed == 2)
					{
						__instance.IsSlideLockUp = true;
					}
					return true;
				}*/
		[HarmonyPatch(typeof(HandgunSlide), "SlideEvent_ArriveAtFore")]
		[HarmonyPostfix]
		static void SlamFireHandgunPatch(HandgunSlide __instance)
		{
			if (enableBrokenFirearmFailures.Value)
			{
				var rand = (float)rnd.Next(0, 10001) / 100;
				if (enableConsoleDebugging.Value) { Debug.Log("Slam fire RNG: " + rand + " to " + SlamfireRate.Value * generalMult.Value); };
				if (rand <= SlamfireRate.Value * generalMult.Value)
				{
					if (enableConsoleDebugging.Value) { Debug.Log("Slam fire!"); };
					__instance.Handgun.DropHammer(false);
				}
			}
		}

		[HarmonyPatch(typeof(ClosedBolt), "BoltEvent_ArriveAtFore")]
		[HarmonyPostfix]
		static void SlamFireClosedBoltPatch(ClosedBolt __instance)
		{
			if (enableBrokenFirearmFailures.Value)
			{
				var rand = (float)rnd.Next(0, 10001) / 100;
				if (enableConsoleDebugging.Value) { Debug.Log("Slam fire RNG: " + rand + " to " + SlamfireRate.Value * generalMult.Value); };
				if (rand <= SlamfireRate.Value * generalMult.Value)
				{
					if (enableConsoleDebugging.Value) { Debug.Log("Slam fire!"); };
					__instance.Weapon.DropHammer();
				}
			}
		}



		[HarmonyPatch(typeof(ClosedBoltWeapon), "CockHammer")]
		[HarmonyPrefix]
		static bool hammerFollowClosedBoltPatch()
		{
			if (!enableBrokenFirearmFailures.Value) { return true; }
			var rand = (float)rnd.Next(0, 10001) / 100;
			if (enableConsoleDebugging.Value) { Debug.Log("Hammer Follow RNG: " + rand + " to " + HammerFollowRate.Value * generalMult.Value); };
			if (rand <= HammerFollowRate.Value * generalMult.Value)
			{
				if (enableConsoleDebugging.Value) { Debug.Log("Hammer follow!"); };
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(Handgun), "CockHammer")]
		[HarmonyPrefix]
		static bool hammerFollowHandgunPatch(bool isManual)
		{
			if (!enableBrokenFirearmFailures.Value) { return true; }
			var rand = (float)rnd.Next(0, 10001) / 100;
			if (enableConsoleDebugging.Value) { Debug.Log("Hammer Follow RNG: " + rand + " to " + HammerFollowRate.Value * generalMult.Value); };
			if (rand <= HammerFollowRate.Value * generalMult.Value && !isManual)
			{
				if (enableConsoleDebugging.Value) { Debug.Log("Hammer follow!"); };
				return false;
			}
			return true;
		}
	}
}
