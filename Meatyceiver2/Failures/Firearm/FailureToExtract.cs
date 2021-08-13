using FistVR;
using HarmonyLib;

namespace Meatyceiver2.Failures.Firearm
{
	public class FailureToExtract
	{
		[HarmonyPatch(typeof(ClosedBolt), "ImpartFiringImpulse")]
		[HarmonyPatch(typeof(HandgunSlide), "ImpartFiringImpulse")]
		[HarmonyPatch(typeof(OpenBoltReceiverBolt), "ImpartFiringImpulse")]
		[HarmonyPrefix]
		static bool GeneralPatch_FailureToExtract(FVRInteractiveObject __instance)
		{
			//these firearms will break if an FTE is caused
			if (__instance is BoltActionRifle || __instance is LeverActionFirearm) return false;
			if (!Meatyceiver.enableFirearmFailures.Value) return true;
			float chance = Meatyceiver.FTERate.Value * Meatyceiver.generalMult.Value;
			if (Meatyceiver.CalcFail(chance))
			{
				__instance.RotationInterpSpeed = 2;
				return false;
			}
//			rand = (float)randomVar.Next(0, 10001) / 100;
//			chance = stovepipeRate.Value * generalMult.Value;
//			consoleDebugging(0, FTEfailureName, rand, chance);
//			if (rand <= chance)
//			{
//				consoleDebugging(1, FTEfailureName, rand, chance);
//				return false;
//			}
			return true;
		}
		
		[HarmonyPatch(typeof(FVRFireArmChamber), "BeginInteraction")] [HarmonyPostfix]
		static void GeneralFixPatch_FailureToExtract(FVRFireArmChamber __instance)
		{
			__instance.RotationInterpSpeed = 1;
		}
		
		[HarmonyPatch(typeof(BreakActionWeapon), "PopOutRound")] [HarmonyPrefix]
		static bool BreakActionPatch_FailureToExtract(BreakActionWeapon __instance, FVRFireArm chamber)
		{
			if (!Meatyceiver.enableFirearmFailures.Value) return true;
			if (chamber.RotationInterpSpeed == 2) return false;
			float chance = Meatyceiver.breakActionFTE.Value
			               * (Meatyceiver.generalMult.Value - 1)
			               *  Meatyceiver.breakActionFTEMultAffect.Value;
			if(Meatyceiver.CalcFail(chance)) {
				chamber.RotationInterpSpeed = 2;
				return false;
			}
			return true;
		}
		
		[HarmonyPatch(typeof(FVRFireArmChamber), "EjectRound")] [HarmonyPrefix]
		static bool RevolverPatch_FailureToExtract(FVRFireArmChamber __instance)
		{
			if (!Meatyceiver.enableFirearmFailures.Value) return true;
			if (__instance.Firearm is Revolver)
			{
				if (__instance.RotationInterpSpeed == 1)
				{
					string failureName = "Revolver FTE";
					float chance = Meatyceiver.revolverFTE.Value * (Meatyceiver.generalMult.Value - 1) * Meatyceiver.revolverFTEGenMultAffect.Value;
					if (Meatyceiver.CalcFail(chance))
					{
						__instance.RotationInterpSpeed = 2;
						return false;
					}
				}
			}
			return true;
		}
		
		[HarmonyPatch(typeof(Revolver), "UpdateCylinderRelease")] [HarmonyPostfix]
		static void RevolverFixPatch_FailureToExtract(Revolver __instance)
		{
			float z = __instance.transform.InverseTransformDirection(__instance.m_hand.Input.VelLinearWorld).z;
			if (z > 0f)
			{
				for (int i = 0; i < __instance.Chambers.Length; i++)
				{
					__instance.Chambers[i].RotationInterpSpeed = 1;
				}
			}
		}
		
		[HarmonyPatch(typeof(FVRFireArmChamber), "EjectRound")] [HarmonyPrefix]
		static bool RollingBlockPatch_FailureToExtract(FVRFireArmChamber __instance)
		{
			if (!Meatyceiver.enableFirearmFailures.Value) return true;
			if (__instance.Firearm is RollingBlock)
			{
				string failureName = "Rolling block FTE";
				float chance = Meatyceiver.breakActionFTE.Value * (Meatyceiver.generalMult.Value - 1) * Meatyceiver.breakActionFTEMultAffect.Value;
				if (Meatyceiver.CalcFail(chance))
					return false;
			}
			return true;
		}
	}
}