using System;
using System.Collections.Generic;
using FistVR;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Meatyceiver2
{
	public class MCM : MonoBehaviour
	{
		public static Dictionary<FVRPhysicalObject, ObjectExtention> objExt;
		
		public static void IncRoundsUsed(FVRPhysicalObject obj)
		{
			CreateKey(obj);
			objExt[obj].roundsUsed++;
		}
		
		//Returns whether it already existed or not.
		public static bool CreateKey(FVRPhysicalObject obj)
		{
			if (objExt.ContainsKey(obj)) return true;
			objExt.Add(obj, new ObjectExtention());
			objExt[obj].naturalReliability = GenerateNatReliability();
			return false;
		}

		public static float GenerateNatReliability(float bias = 0)
		{
			float random = Random.Range(0f, 1f);
			//0.15x^2+1
			float mult = Meatyceiver.NatReliabilityModifier.Value * (Mathf.Pow(random, 2)) + 1;
			return mult;
		}

		public static float GetMultForRoundsUsed(FVRPhysicalObject obj)
		{
			CreateKey(obj);
			float mult = 1;
			var r = objExt[obj].roundsUsed;
			if (obj is FVRFireArm)
			{
				//TODO: MAKE THIS OPTIONS!!!!
				mult += (0.0001f * (r - 400f));
				mult = Mathf.Clamp(mult, 1f, 1.1f);
			}

			return mult;
		}
	}
	
	[Serializable]
	public class ObjectExtention
	{
		public float naturalReliability; //lower = more reliable; normal is 1
		public int roundsUsed;
	}
}