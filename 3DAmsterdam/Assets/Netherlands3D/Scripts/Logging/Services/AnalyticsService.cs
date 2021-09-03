using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Logging.Services
{
	[System.Serializable]
	public class AnalyticsService : MonoBehaviour
	{
		public virtual void SendEvent(string category, string action, string label = "") { }

		private void OnEnable()
		{
			Debug.Log("Analytics service enabled");
		}
		private void OnDisable()
		{
			Debug.Log("Analytics service disabled");
		}
	}
}