using Netherlands3D.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
//using UnityEngine.Analytics;

//Siteimprove.com used by Amsterdam as a Analytics Service
namespace Netherlands3D.Logging.Services
{
	public class SiteImproveAnalyticsService : AnalyticsService
	{
		[DllImport("__Internal")]
		private static extern void AutoPauseLogging();
		[DllImport("__Internal")]
		private static extern void PushEvent(string category = "category", string action = "action", string label = "label");
#if !UNITY_EDITOR && UNITY_WEBGL
		private void Start()
		{
			AutoPauseLogging();
		}
#endif
		public override void SendEvent(string category, string action, string label = "")
		{		
#if !UNITY_EDITOR && UNITY_WEBGL
			PushEvent(category, action, label);
#endif
		}
	}
}