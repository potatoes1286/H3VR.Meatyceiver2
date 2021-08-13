using System.Resources;
using HarmonyLib;
using FistVR;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using Meatyceiver2.Failures;
using Meatyceiver2.Failures.Ammo;
using Meatyceiver2.Failures.Breakage;
using Meatyceiver2.Failures.Firearm;

namespace Meatyceiver2
{
	[BepInPlugin("dll.potatoes.meatyceiver2", "Meatyceiver2", "0.3.3")]
	public class Meatyceiver : BaseUnityPlugin
	{
		private ResourceManager stringManager = new ResourceManager(typeof(Resources));
		//General Settings

		public static ConfigEntry<bool> enableFirearmFailures;
		public static ConfigEntry<bool> enableAmmunitionFailures;
		public static ConfigEntry<bool> enableBrokenFirearmFailures;
		public static ConfigEntry<bool> enableConsoleDebugging;

		//Multipliers

		public static ConfigEntry<float> generalMult;

		//Secondary Failure - Mag Unreliability

		public static ConfigEntry<bool> enableMagUnreliability;
		public static ConfigEntry<float> magUnreliabilityGenMultAffect;
		public static ConfigEntry<float> failureIncPerRound;
		public static ConfigEntry<int> minRoundCount;

		//Secondary Failure - Long Term Breakdown

		public static ConfigEntry<bool> enableLongTermBreakdown;
		public static ConfigEntry<float> maxFirearmFailureInc;
		public static ConfigEntry<float> maxBrokenFirearmFailureInc;
		public static ConfigEntry<float> longTermBreakdownGenMultAffect;
		public static ConfigEntry<int> roundsTillMaxBreakdown;


		//Failures - Ammo

		public static ConfigEntry<float> LPSFailureRate;
		public static ConfigEntry<float> handFireRate;

		//Failures - Firearms

		public static ConfigEntry<float> FTFRate;
		public static ConfigEntry<float> FTERate;
		public static ConfigEntry<float> DFRate;
		public static ConfigEntry<float> stovepipeRate;
		public static ConfigEntry<float> stovepipeLerp;

		//Failures - Broken Firearm

		public static ConfigEntry<float> HFRate;
		public static ConfigEntry<float> FTLSlide;
		public static ConfigEntry<float> slamfireRate;


		//Bespoke Failures

		public static ConfigEntry<float> breakActionFTE;
		public static ConfigEntry<float> breakActionFTEMultAffect;

		public static ConfigEntry<float> revolverFTE;
		public static ConfigEntry<float> revolverFTEGenMultAffect;
//		public static ConfigEntry<float> revolverFTEshakeMult;

		public static System.Random randomVar;

		void Awake()
		{
			Logger.LogInfo("Meatyceiver2 started!");
			InitFields();
			InitPatches();
			randomVar = new System.Random();
		}

		public void InitFields()
		{ 
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
			revolverFTE = Config.Bind(Strings.BespokeFailure, Strings.RevolverFTE_key, 30f, Strings.ValidInput_float);
			revolverFTEGenMultAffect = Config.Bind(Strings.BespokeFailure, Strings.RevolverFTERate_key, 0.5f, Strings.FTEMult_description);

		}

		public void InitPatches()
		{
			//ammo
			Harmony.CreateAndPatchAll(typeof(LightPrimerStrike));
			//breakage
			Harmony.CreateAndPatchAll(typeof(FailureToLockSlide));
			Harmony.CreateAndPatchAll(typeof(HammerFollow));
			Harmony.CreateAndPatchAll(typeof(Slamfire));
			//firearm
			Harmony.CreateAndPatchAll(typeof(FailureToExtract));
			Harmony.CreateAndPatchAll(typeof(FailureToFire));
			//other
			Harmony.CreateAndPatchAll(typeof(OtherFailures));
			Harmony.CreateAndPatchAll(typeof(Meatyceiver));
		}
		
		public static bool CalcFail(float chance)
		{
			//effectively returns a number between 0 and 100 with 2 decimal points
			float rand = (float)randomVar.Next(0, 10001) / 100;
			if (chance <= rand) return true;
			return false;
		}
		


		
		//hammer follow. not sure why this was disabled?
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
		
		//stovepipe fuckery. not working
		/*[HarmonyPatch(typeof(HandgunSlide), "UpdateSlide")]
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
		}*/
		
		//believe this also has to do with stovepipes
		/*[HarmonyPatch(typeof(HandgunSlide), "UpdateSlide")]
		[HarmonyPostfix]
		static void SPHandgunSlideFix(HandgunSlide __instance, float ___m_slideZ_current, float __state)
		{
			//			if (__instance.RotationInterpSpeed == 2) Debug.Log("prefix slidez: " + __state + " postfix slidez: " + ___m_slideZ_current);
			if (__instance.GameObject.transform.localPosition.z >= __state && __instance.RotationInterpSpeed == 2)
			{
				__instance.GameObject.transform.localPosition = new Vector3(__instance.GameObject.transform.localPosition.x, __instance.GameObject.transform.localPosition.y, __state);
				__instance.Handgun.Chamber.UpdateProxyDisplay();
			}
		}*/
		
		//this is def part of stovepiping
		/*[HarmonyPatch(typeof(Handgun), "UpdateDisplayRoundPositions")]
		[HarmonyPostfix]
		static void SPHandgun(Handgun __instance, FVRFirearmMovingProxyRound ___m_proxy)
		{
			if (__instance.Slide.RotationInterpSpeed == 2)
			{
				Debug.Log("lerping");
				___m_proxy.ProxyRound.transform.localPosition = Vector3.Lerp(__instance.Slide.Point_Slide_Forward.transform.position, __instance.Slide.Point_Slide_Rear.transform.position, stovepipeLerp.Value);
			}
		}*/
	}
}
