using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace Netherlands3D.Logging.Services
{
	[System.Serializable]
	public class AnalyticsService : MonoBehaviour
	{
		public virtual void SendEvent(string eventName, Dictionary<string, object> eventData) { }
	}
}