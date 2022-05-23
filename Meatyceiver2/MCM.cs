using System;
using System.Collections.Generic;
using Alloy;
using FistVR;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Meatyceiver2
{
	public class MCM : MonoBehaviour
	{
		public static Dictionary<FVRInteractiveObject, ObjectExtention> objExt;
		
		public static void IncRoundsUsed(FVRInteractiveObject obj) {
			CreateKey(obj);
			objExt[obj].roundsUsed++;
		}
		public static void AddFlag(FVRInteractiveObject obj, states state) { CreateKey(obj); objExt[obj].state |= state; }
		public static void RemoveFlag(FVRInteractiveObject obj, states state) { CreateKey(obj); objExt[obj].state &= ~state; }
		public static bool HasFlag(FVRInteractiveObject obj, states state) { CreateKey(obj); return objExt[obj].state.HasFlag(state); }
		public static float GetParabola(float a, float b, float c, float x) { return a*x*x + b*x + c; }
		
		//Returns whether it already existed or not.
		public static bool CreateKey(FVRInteractiveObject obj)
		{
			if (objExt.ContainsKey(obj)) return true;
			objExt.Add(obj, new ObjectExtention());
			objExt[obj].naturalReliability = GenerateNatReliability();
			return false;
		}

		public static float GenerateNatReliability(float bias = 0)
		{
			float random = Random.Range(-1f, 1f) + bias; //bias can make it go over 1 or -1. careful!
			float mult = 1 + Meatyceiver.NatReliabilityModifier.Value * 0.15f * Mathf.Pow(random, 3); //0.15x^3
			return mult;
		}

		public static float GetMultForRoundsUsed(FVRInteractiveObject obj)
		{
			CreateKey(obj);
			float mult = 0;
			var r = objExt[obj].roundsUsed;
			if (obj is FVRFireArm)
			{
				//TODO: MAKE THIS OPTIONS!!!!
				mult = 1 + Mathf.Min((0.0001f * (r - 400f)), 0.1f);
			}

			return mult;
		}
	}
	
	[Serializable]
	public class ObjectExtention
	{
		public float naturalReliability; //lower = more reliable; normal is 1
		public int roundsUsed;
		public states state;
	}
	
	[Flags]
	public enum states
	{
		RunawayGun,
		StuckRound,
		BrokenHammer
	}
}