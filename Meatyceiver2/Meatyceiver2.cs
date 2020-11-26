using System.Resources;
using HarmonyLib;
using FistVR;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

namespace Meatyceiver2
{
	[BepInPlugin("dll.potatoes.meatyceiver2", "Meatyceiver2", "0.2.10")]
	public class Meatyceiver : BaseUnityPlugin
	{
		private ResourceManager stringManager = new ResourceManager(typeof(Resources));
		//General Settings

		private static ConfigEntry<bool> enableFirearmFailures;
		private static ConfigEntry<bool> enableAmmunitionFailures;
		private static ConfigEntry<bool> enableBrokenFirearmFailures;
		private static ConfigEntry<bool> enableConsoleDebugging;

		//Multipliers

		private static ConfigEntry<float> generalMult;

		//Secondary Failure - Mag Unreliability

		private static ConfigEntry<bool> enableMagUnreliability;
		private static ConfigEntry<float> magUnreliabilityGenMultAffect;
		private static ConfigEntry<float> failureIncPerRound;
		private static ConfigEntry<int> minRoundCount;

		//Secondary Failure - Long Term Breakdown

		private static ConfigEntry<bool> enableLongTermBreakdown;
		private static ConfigEntry<float> maxFirearmFailureInc;
		private static ConfigEntry<float> maxBrokenFirearmFailureInc;
		private static ConfigEntry<float> longTermBreakdownGenMultAffect;
		private static ConfigEntry<int> roundsTillMaxBreakdown;


		//Failures - Ammo

		private static ConfigEntry<float> LPSFailureRate;
		private static ConfigEntry<float> handFireRate;

		//Failures - Firearms

		private static ConfigEntry<float> FTFRate;
		private static ConfigEntry<float> FTERate;
		private static ConfigEntry<float> DFRate;
		private static ConfigEntry<float> stovepipeRate;
		private static ConfigEntry<float> stovepipeLerp;

		//Failures - Broken Firearm

		private static ConfigEntry<float> HFRate;
		private static ConfigEntry<float> FTLSlide;
		private static ConfigEntry<float> slamfireRate;


		//Bespoke Failures

		private static ConfigEntry<float> breakActionFTE;
		private static ConfigEntry<float> breakActionFTEMultAffect;

		private static ConfigEntry<float> bespokeFailureRevolverFTE;
		private static ConfigEntry<float> bespokeFailureRevolverFTEGenMultAffect;

		public static System.Random randomVar;

		void Awake()
		{
	
			Logger.LogInfo("Meatyceiver2 started!");
			
			enableAmmunitionFailures = Config.Bind(Strings.GeneralSettings, Strings.EnableAmmunitionFailures_key, true, Strings.EnableAmmunitionFailures_description);
			enableFirearmFailures = Config.Bind(Strings.GeneralSettings, Strings.EnableFirearmFailures_key, true, Strings.EnableFirearmFailures_description);
			enableBrokenFirearmFailures = Config.Bind(Strings.GeneralSettings, Strings.EnableBrokenFirearmFailures_key, true, Strings.EnableBrokenFirearmFailures_description);
			enableConsoleDebugging = Config.Bind(Strings.GeneralSettings,Strings.EnableConsoleDebugging_key, false, Strings.EnableConsoleDebugging_description);

			generalMult = Config.Bind(Strings.GeneralMultipliers_section, Strings.GeneralMultipliers_key, 1f, Strings.GeneralMultipliers_description);


			enableMagUnreliability = Config.Bind(Strings.MagUnreliability_section, Strings.MagReliability_key, true, Strings.MagReliability_description);
			failureIncPerRound = Config.Bind(Strings.MagUnreliability_section, Strings.MagReliabilityMult_key, 0.04f, Strings.MagReliabilityMult_description);
			minRoundCount = Config.Bind(Strings.MagUnreliability_section, Strings.MinRoundCount_key, 15, Strings.MinRoundCount_description);
			magUnreliabilityGenMultAffect = Config.Bind(Strings.MagUnreliability_section, Strings.MagUnreliabilityMult_key, 0.5f, Strings.MagUnreliabilityMult_description);

			//enableLongTermBreakdown = Config.Bind(Strings.LongTermBreak_section, Strings.LongTermBreak_key, true, Strings.LongTermBreak_description);

			LPSFailureRate = Config.Bind(Strings.AmmoFailures_section, Strings.LPSRate_key, 0.25f, Strings.ValidInput_float);
			handFireRate = Config.Bind(Strings.AmmoFailures_section, Strings.HangFireRate_key, 0.1f, Strings.ValidInput_float);

			FTFRate = Config.Bind(Strings.FirearmFailures_section, Strings.FTFRate_key, 0.25f, Strings.ValidInput_float);
			FTERate = Config.Bind(Strings.FirearmFailures_section, Strings.FTERate_key, 0.15f, Strings.ValidInput_float);
			DFRate = Config.Bind(Strings.FirearmFailures_section, Strings.DFRate_key, 0.15f, Strings.ValidInput_float);
			stovepipeRate = Config.Bind(Strings.FirearmFailures_section, Strings.StovepipeRate_key, 0.1f, Strings.ValidInput_float);
			stovepipeLerp = Config.Bind(Strings.FirearmFailures_section, Strings.StovepipeLerp_key, 0.5f, Strings.DEBUG);

			HFRate = Config.Bind(Strings.BrokenFirearmFailure, Strings.HFRate_key, 0.1f, Strings.ValidInput_float);
			FTLSlide = Config.Bind(Strings.BrokenFirearmFailure, Strings.FTLSlide_key, 5f, Strings.ValidInput_float);
			slamfireRate = Config.Bind(Strings.BrokenFirearmFailure, Strings.SlamFireRate_key, 0.1f, Strings.ValidInput_float);

			breakActionFTE = Config.Bind(Strings.BespokeFailure, Strings.BreakActionFTE_key, 30f, Strings.ValidInput_float);
			breakActionFTEMultAffect = Config.Bind(Strings.BespokeFailure, Strings.BreakActionFTEMult_key,  0.5f, Strings.FTEMult_description);
			bespokeFailureRevolverFTE = Config.Bind(Strings.BespokeFailure, Strings.RevolverFTE_key, 30f, Strings.ValidInput_float);
			bespokeFailureRevolverFTEGenMultAffect = Config.Bind(Strings.BespokeFailure, Strings.RevolverFTERate_key, 0.5f, Strings.FTEMult_description);



			Harmony.CreateAndPatchAll(typeof(Meatyceiver));
			randomVar = new System.Random();
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
		static bool LightPrimerStrike(ref bool __result, FVRFireArmChamber __instance, FVRFireArmRound ___m_round)
		{
			string failureName = "LPS";
			if (!enableAmmunitionFailures.Value) return true;
			if (__instance.Firearm is Revolver || __instance.Firearm is RevolvingShotgun) return true;
			float rand = (float)randomVar.Next(0, 10001) / 100;
			float chance = LPSFailureRate.Value * generalMult.Value;
			consoleDebugging(0, failureName, rand, chance);
			//			if (enableConsoleDebugging.Value) { Debug.Log("LPS RNG: " + rand + " to " + LPSFailureRate.Value * generalMult.Value); }
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
		static bool LPSRevolver(Revolver __instance)
		{
			string failureName = "LPS";
			if (!enableAmmunitionFailures.Value) { return true; }
			float rand = (float)randomVar.Next(0, 10001) / 100;
			float chance = LPSFailureRate.Value * generalMult.Value;
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
		static bool LPSRevolvingShotgun(RevolvingShotgun __instance)
		{
			string failureName = "LPS";
			if (!enableAmmunitionFailures.Value) { return true; }
			float rand = (float)randomVar.Next(0, 10001) / 100;
			float chance = LPSFailureRate.Value * generalMult.Value;
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
			var rand = (float)randomVar.Next(0, 10001) / 100;
			if (__instance.Magazine != null && enableMagUnreliability.Value)
			{
				if (!__instance.Magazine.IsBeltBox)
				{
					failureinc = (float)((__instance.Magazine.m_capacity - minRoundCount.Value) * failureIncPerRound.Value) * (generalMult.Value * magUnreliabilityGenMultAffect.Value);
				}
			}
			float chance = HFRate.Value * generalMult.Value + failureinc;
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
		static bool FTPEmptyBreakAction(BreakActionWeapon __instance, FVRFireArm chamber)
		{
			string failureName = "BA FTE";
			if (!enableFirearmFailures.Value) return true;
			if (chamber.RotationInterpSpeed == 2) return false;
			float rand = (float)randomVar.Next(0, 10001) / 100;
			float chance = breakActionFTE.Value * (generalMult.Value * breakActionFTEMultAffect.Value);
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
		static void BreakActionFTEFix(FVRFireArmChamber __instance)
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
			float rand = (float)randomVar.Next(0, 10001) / 100;
			float chance = stovepipeRate.Value * generalMult.Value;
			consoleDebugging(0, StovePipeFailureName, rand, chance);
			if (rand <= chance)
			{
				consoleDebugging(1, StovePipeFailureName, rand, chance);
				__instance.RotationInterpSpeed = 2;
				return false;
			}
			rand = (float)randomVar.Next(0, 10001) / 100;
			chance = stovepipeRate.Value * generalMult.Value;
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
		static bool SPHandgunSlide(
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
				___m_slideZ_current = ___m_slideZ_forward - (___m_slideZ_forward - ___m_slideZ_rear) / 2;
				Debug.Log("prefix slidez: " + ___m_slideZ_current);
				___m_curSlideSpeed = 0;
				if (__instance.CurPos == HandgunSlide.SlidePos.LockedToRear)
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
		static void SPHandgunSlideFix(HandgunSlide __instance, float ___m_slideZ_current, float __state)
		{
			//			if (__instance.RotationInterpSpeed == 2) Debug.Log("prefix slidez: " + __state + " postfix slidez: " + ___m_slideZ_current);
			if (__instance.GameObject.transform.localPosition.z >= __state && __instance.RotationInterpSpeed == 2)
			{
				__instance.GameObject.transform.localPosition = new Vector3(__instance.GameObject.transform.localPosition.x, __instance.GameObject.transform.localPosition.y, __state);
				__instance.Handgun.Chamber.UpdateProxyDisplay();
			}
		}

		[HarmonyPatch(typeof(Handgun), "UpdateDisplayRoundPositions")]
		[HarmonyPostfix]
		static void SPHandgun(Handgun __instance, FVRFirearmMovingProxyRound ___m_proxy)
		{
			if (__instance.Slide.RotationInterpSpeed == 2)
			{
				Debug.Log("lerping");
				___m_proxy.ProxyRound.transform.localPosition = Vector3.Lerp(__instance.Slide.Point_Slide_Forward.transform.position, __instance.Slide.Point_Slide_Rear.transform.position, stovepipeLerp.Value);
			}
		}

		/*		[HarmonyPatch(typeof(FVRFireArmChamber), "EjectRound")]
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

		/*		[HarmonyPatch(typeof(FVRFireArmRound), "FVRFixedUpdate")]
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
				}*/



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
		static void SFHandgun(HandgunSlide __instance)
		{
			if (enableBrokenFirearmFailures.Value)
			{
				string failureName = "Slam fire";
				float rand = (float)randomVar.Next(0, 10001) / 100;
				float chance = slamfireRate.Value * generalMult.Value;
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
		static void SFClosedBolt(ClosedBolt __instance)
		{
			if (enableBrokenFirearmFailures.Value)
			{
				string failureName = "Slam fire";
				float rand = (float)randomVar.Next(0, 10001) / 100;
				float chance = slamfireRate.Value * generalMult.Value;
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
		static bool HFClosedBolt()
		{
			if (!enableBrokenFirearmFailures.Value) { return true; }
			string failureName = "Hammer follow";
			float rand = (float)randomVar.Next(0, 10001) / 100;
			float chance = HFRate.Value * generalMult.Value;
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
		static bool HFHandgun(bool isManual)
		{
			if (!enableBrokenFirearmFailures.Value) { return true; }
			string failureName = "Hammer follow";
			float rand = (float)randomVar.Next(0, 10001) / 100;
			float chance = HFRate.Value * generalMult.Value;
			consoleDebugging(0, failureName, rand, chance);
			if (rand <= chance && !isManual)
			{
				consoleDebugging(1, failureName, rand, chance);
				return false;
			}
			return true;
		}
	
		[HarmonyPatch(typeof(Handgun), "EngageSlideRelease")]
		[HarmonyPrefix]
		static bool FTLSHandgun()
		{
			if (!enableBrokenFirearmFailures.Value) return true;
			string failureName = "Failure to lock slide";
			float rand = (float)randomVar.Next(0, 10001) / 100;
			float chance = FTLSlide.Value * generalMult.Value;
			consoleDebugging(0, failureName, rand, chance);
			if (rand <= chance)
			{
				consoleDebugging(1, failureName, rand, chance);
				return false;
			}
			return true;
		}
	}
}
