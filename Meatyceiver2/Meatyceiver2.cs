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
	[BepInPlugin("dll.potatoes.meatyceiver2", "Meatyceiver2", "0.2.7")]
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
		private static ConfigEntry<float> BespokeFailureBreakActionShotgunFTEGenMultAffect;

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

			HammerFollowRate = Config.Bind("Failures - Broken Firearm", "Hammer Follow Rate", 0.1f, "Valid numbers are 0-100");
			failureToLockSlide = Config.Bind("Failures - Broken Firearm", "Failure to Lock Slide Rate", 0.3f, "Valid numbers are 0-100");
			SlamfireRate = Config.Bind("Failures - Broken Firearm", "Slam Fire Rate", 0.1f, "Valid numbers are 0-100");

			BespokeFailureBreakActionShotgunFTE = Config.Bind("Failures - Bespoke", "Break Action Failure To Eject", 20f, "Valid numbers are 0-100. By default, GenMult applies to this 50%.");
			BespokeFailureBreakActionShotgunFTEGenMultAffect = Config.Bind("Failures - Bespoke", "Break Action Failure To Eject General Multiplier Affect", 0.5f, "General Multiplier is multiplied by this before affecting BA FTE.");

			Harmony.CreateAndPatchAll(typeof(Meatyceiver));
			rnd = new System.Random();
		}


		public static void consoleDebugging(short responseType, string _failName, float _rand, float _percentChance)
		{
			if (!enableConsoleDebugging.Value) return;
			switch (responseType)
			{
				case 0:
					Debug.Log(_failName + " RandomNum: " + _rand + " to " + _percentChance);
					break;
				case 1:
					Debug.Log(_failName + " failure!");
					break;
			}
		}












		//BEGIN AMMO FAILURES

		[HarmonyPatch(typeof(FVRFireArmChamber), "Fire")]
		[HarmonyPrefix]
		static bool LightPrimerStrikePatch(ref bool __result, FVRFireArmChamber __instance, FVRFireArmRound ___m_round)
		{
			string failureName = "LPS";
			if (!enableAmmunitionFailures.Value) return true;
			if (__instance.Firearm is Revolver || __instance.Firearm is RevolvingShotgun) return true;
			float rand = (float)rnd.Next(0, 10001) / 100;
			float chance = lightPrimerStrikeFailureRate.Value * generalMult.Value;
			consoleDebugging(0, failureName, rand, chance);
			//			if (enableConsoleDebugging.Value) { Debug.Log("LPS RNG: " + rand + " to " + lightPrimerStrikeFailureRate.Value * generalMult.Value); }
			if (rand >= chance)
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
				consoleDebugging(1, failureName, rand, chance);
			}
			__result = false;
			return false;
		}

		[HarmonyPatch(typeof(Revolver), "Fire")]
		[HarmonyPrefix]
		static bool LightPrimerStrikeRevolverPatch(Revolver __instance)
		{
			string failureName = "LPS";
			if (!enableAmmunitionFailures.Value) { return true; }
			float rand = (float)rnd.Next(0, 10001) / 100;
			float chance = lightPrimerStrikeFailureRate.Value * generalMult.Value;
			consoleDebugging(0, failureName, rand, chance);
			if (rand <= chance)
			{
				consoleDebugging(1, failureName, rand, chance);
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
			string failureName = "LPS";
			if (!enableAmmunitionFailures.Value) { return true; }
			float rand = (float)rnd.Next(0, 10001) / 100;
			float chance = lightPrimerStrikeFailureRate.Value * generalMult.Value;
			consoleDebugging(0, failureName, rand, chance);
			if (rand <= chance)
			{
				consoleDebugging(1, failureName, rand, chance);
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
			string failureName = "FTF";
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
			float chance = (HammerFollowRate.Value + failureinc) * generalMult.Value;
			consoleDebugging(0, failureName, rand, chance);
			if (rand <= chance)
			{
				consoleDebugging(1, failureName, rand, chance);
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(BreakActionWeapon), "PopOutRound")]
		[HarmonyPrefix]
		static bool FailtoPopEmptyBreakActionPatch(BreakActionWeapon __instance, FVRFireArm chamber)
		{
			string failureName = "BA FTE";
			if (!enableFirearmFailures.Value) return true;
			if (chamber.RotationInterpSpeed == 2) return false;
			float rand = (float)rnd.Next(0, 10001) / 100;
			float chance = BespokeFailureBreakActionShotgunFTE.Value * (generalMult.Value * BespokeFailureBreakActionShotgunFTEGenMultAffect.Value);
			consoleDebugging(0, failureName, rand, chance);
			if (rand <= chance)
			{
				consoleDebugging(1, failureName, rand, chance);
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
			string FTEfailureName = "FTE";
			string StovePipeFailureName = "Stovepipe";
			if (__instance is BoltActionRifle) { return false; }
			if (__instance is LeverActionFirearm) { return false; }
			if (!enableFirearmFailures.Value) { return true; }
			float rand = (float)rnd.Next(0, 10001) / 100;
			float chance = StovepipeRate.Value * generalMult.Value;
			consoleDebugging(0, StovePipeFailureName, rand, chance);
			if (rand <= chance)
			{
				consoleDebugging(1, StovePipeFailureName, rand, chance);
				__instance.RotationInterpSpeed = 2;
				return true;
			}
			rand = (float)rnd.Next(0, 10001) / 100;
			chance = StovepipeRate.Value * generalMult.Value;
			consoleDebugging(0, FTEfailureName, rand, chance);
			if (rand <= chance)
			{
				consoleDebugging(1, FTEfailureName, rand, chance);
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(HandgunSlide), "UpdateSlide")]
		[HarmonyPrefix]
		static bool StovePipeHandgunSlidePatch(
			HandgunSlide __instance,
			float ___m_slideZ_forward,
			float ___m_slideZ_rear,
			float ___m_slideZ_current,
			float ___m_curSlideSpeed,
			out float __state
			)
		{
			if (__instance.RotationInterpSpeed == 2)
			{
/*				if (___m_slideZ_current == null) Debug.Log("current nbull");
				if (___m_slideZ_forward == null) Debug.Log("forward nbull");
				if (___m_slideZ_rear == null) Debug.Log("rear nbull");*/

				___m_slideZ_current = ___m_slideZ_forward - (___m_slideZ_forward - ___m_slideZ_rear) / 2;
				Debug.Log("prefix slidez: " + ___m_slideZ_current);
				___m_curSlideSpeed = 0;
				if (__instance.CurPos == HandgunSlide.SlidePos.Rear)
				{
					__instance.RotationInterpSpeed = 1;
					Debug.Log("Stovepipe cleared!");
				}
			}
			__state = ___m_slideZ_current;
			return true;
		}

		[HarmonyPatch(typeof(HandgunSlide), "UpdateSlide")]
		[HarmonyPostfix]
		static void StovePipeHandgunSlidePostfixPatch(HandgunSlide __instance, float ___m_slideZ_current, float __state)
		{
			//			if (__instance.RotationInterpSpeed == 2) Debug.Log("prefix slidez: " + __state + " postfix slidez: " + ___m_slideZ_current);
			if (__instance.GameObject.transform.localPosition.z >= __state && __instance.RotationInterpSpeed == 2)
			{
				__instance.GameObject.transform.localPosition = new Vector3(__instance.GameObject.transform.localPosition.x, __instance.GameObject.transform.localPosition.y, __state);
			}
		}

		[HarmonyPatch(typeof(FVRFireArmChamber), "EjectRound")]
		[HarmonyPrefix]
		static bool StovePipeHandgunEjectExtractedRoundPatch(FVRFireArmRound __result, FVRFireArmChamber __instance, FVRFireArmRound ___m_round, Vector3 EjectionPosition, Vector3 EjectionVelocity, Vector3 EjectionAngularVelocity, bool ForceCaseLessEject = false)
		{
			if (___m_round != null)
			{
				bool flag = false;
				if (__instance.Firearm != null)
				{
					flag = true;
					if (__instance.Firearm.HasImpactController)
					{
						__instance.Firearm.AudioImpactController.SetCollisionsTickDownMax(0.2f);
					}
				}
				FVRFireArmRound fvrfireArmRound = null;
				if (!___m_round.IsCaseless || ForceCaseLessEject)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(___m_round.gameObject, EjectionPosition, __instance.transform.rotation);
					fvrfireArmRound = gameObject.GetComponent<FVRFireArmRound>();
					if (flag)
					{
						fvrfireArmRound.SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
					}
					fvrfireArmRound.RootRigidbody.velocity = Vector3.Lerp(EjectionVelocity * 0.7f, EjectionVelocity, UnityEngine.Random.value) + GM.CurrentMovementManager.GetFilteredVel();
					fvrfireArmRound.RootRigidbody.maxAngularVelocity = 200f;
					fvrfireArmRound.RootRigidbody.angularVelocity = Vector3.Lerp(EjectionAngularVelocity * 0.3f, EjectionAngularVelocity, UnityEngine.Random.value);
					if (__instance.IsSpent)
					{
						fvrfireArmRound.SetKillCounting(true);
						fvrfireArmRound.Fire();
					}

					if (__instance.Firearm is Handgun) {
						var handgunSlide = __instance.Firearm.transform.GetComponent<Handgun>().Slide;
						if (handgunSlide.RotationInterpSpeed == 2)
						{
							gameObject.GetComponent<FVRFireArmRound>().RootRigidbody.isKinematic = true;
							gameObject.transform.SetParent(handgunSlide.transform, true);
							gameObject.transform.position = Vector3.Lerp(handgunSlide.Point_Slide_Forward.transform.position, handgunSlide.Point_Slide_Rear.transform.position, 0.2f);
						}
					}
				}
				__instance.SetRound(null);
				__result = fvrfireArmRound;
				return false;
			}
			__result = null;
			return false;
		}
		/*		static void StovePipeHandgunEjectExtractedRoundPatch(FVRFireArmChamber __instance, GameObject gameObject)
				{
					if (__instance.Firearm is Handgun) return;
					var handgunSlide = __instance.Firearm.transform.GetComponent<Handgun>().Slide;
					if (handgunSlide.RotationInterpSpeed == 2)
					{
						gameObject.GetComponent<FVRFireArmRound>().RootRigidbody.isKinematic = true;
						gameObject.transform.SetParent(handgunSlide.transform, true);
						gameObject.transform.position = Vector3.Lerp(handgunSlide.Point_Slide_Forward.transform.position, handgunSlide.Point_Slide_Rear.transform.position, 0.2f);
					}
				}*/

		[HarmonyPatch(typeof(FVRFireArmRound), "FVRFixedUpdate")]
		[HarmonyPrefix]
		static bool StovePipeFVRFireArmRoundPatch(FVRFireArmRound __instance)
		{
			if (__instance.RootRigidbody.isKinematic)
			{
				var hgslide = __instance.Transform.parent.GetComponent<HandgunSlide>();
				if (__instance.IsHeld == true || hgslide.RotationInterpSpeed == 1)
				{
					__instance.RootRigidbody.isKinematic = false;
					hgslide.RotationInterpSpeed = 1;
					Debug.Log("Stovepipe cleared!");
					__instance.Transform.parent = null;
				}
			}
			return true;
		}



		/*		[HarmonyPatch(typeof(Handgun), "Awake")]
				[HarmonyPrefix]
				static bool addStovePipeScript(Handgun __instance)
				{
					__instance.GameObject.AddComponent<StovePipe>();
					return true;
				}*/

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
				string failureName = "Slam fire";
				float rand = (float)rnd.Next(0, 10001) / 100;
				float chance = SlamfireRate.Value * generalMult.Value;
				consoleDebugging(0, failureName, rand, chance);
				if (rand <= chance)
				{
					consoleDebugging(1, failureName, rand, chance);
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
				string failureName = "Slam fire";
				float rand = (float)rnd.Next(0, 10001) / 100;
				float chance = SlamfireRate.Value * generalMult.Value;
				consoleDebugging(0, failureName, rand, chance);
				if (rand <= chance)
				{
					consoleDebugging(1, failureName, rand, chance);
					__instance.Weapon.DropHammer();
				}
			}
		}



		[HarmonyPatch(typeof(ClosedBoltWeapon), "CockHammer")]
		[HarmonyPrefix]
		static bool hammerFollowClosedBoltPatch()
		{
			if (!enableBrokenFirearmFailures.Value) { return true; }
			string failureName = "Hammer follow";
			float rand = (float)rnd.Next(0, 10001) / 100;
			float chance = HammerFollowRate.Value * generalMult.Value;
			consoleDebugging(0, failureName, rand, chance);
			if (rand <= chance)
			{
				consoleDebugging(1, failureName, rand, chance);
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(Handgun), "CockHammer")]
		[HarmonyPrefix]
		static bool hammerFollowHandgunPatch(bool isManual)
		{
			if (!enableBrokenFirearmFailures.Value) { return true; }
			string failureName = "Hammer follow";
			float rand = (float)rnd.Next(0, 10001) / 100;
			float chance = HammerFollowRate.Value * generalMult.Value;
			consoleDebugging(0, failureName, rand, chance);
			if (rand <= chance && !isManual)
			{
				consoleDebugging(1, failureName, rand, chance);
				return false;
			}
			return true;
		}
	}
}
