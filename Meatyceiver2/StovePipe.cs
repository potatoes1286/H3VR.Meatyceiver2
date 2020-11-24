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
	class StovePipe : MonoBehaviour
	{
		public Handgun firearm;

		public bool inStovePipe;

		public float m_boltForward;
		public float m_boltRear;
		public float m_boltStovePipe;

		void Start()
		{
			firearm = this.GetComponent<Handgun>();
		}

		void Update()
		{
			if (firearm.RotationInterpSpeed == 2) inStovePipe = true;

			if (inStovePipe)
			{
				m_boltForward = firearm.Slide.Point_Slide_Forward.transform.position.z;
				m_boltRear = firearm.Slide.Point_Slide_Rear.transform.position.z;
//				firearm.Slide.transform.position 
			}

		}
	}
}
