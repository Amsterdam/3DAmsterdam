using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Analytics;

/* Uncomment 'using UnityEngine.Analytics; '
 * if you want to use Unity Analytics.
 * Commenting it out will make sure it is excluded from the build.
 */

namespace Netherlands3D.Logging.Services
{
	public class UnityAnalyticsService : AnalyticsService
	{
		private void Start()
		{
			//Debug.Log("Privacy URL");
			//DataPrivacy.FetchPrivacyUrl((action) => { Debug.Log(action); });
		}

		public override void SendEvent(string eventName, Dictionary<string, object> eventData)
		{
#if !UNITY_EDITOR && UNITY_ANALYTICS
			//Unity's own Analytics service events:
			UnityEngine.Analytics.Analytics.CustomEvent(eventName, eventData);
			UnityEngine.Analytics.Analytics.FlushEvents();
#endif
		}
	}
}