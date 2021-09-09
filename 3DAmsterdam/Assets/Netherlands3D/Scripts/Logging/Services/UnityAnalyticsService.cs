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

		public override void SendEvent(string category, string action, string label = "")
		{
#if !UNITY_EDITOR && UNITY_ANALYTICS
			var eventName = action;
			Dictionary<string, object> eventData = new Dictionary<string, object>
			{
				{ "category", category },
				{ "label", label },
			};
			//Unity's own Analytics service events:
			UnityEngine.Analytics.Analytics.CustomEvent(eventName, eventData);
			UnityEngine.Analytics.Analytics.FlushEvents();
#endif
		}
	}
}