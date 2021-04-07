#if UNITY_EDITOR || !PRODUCTION
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace Amsterdam3D.Logging
{
    public class AnalyticsStartup
    {
        /// <summary>
        /// This entire class only executes only gets compiled if we are not in PRODUCTION.
        /// This allows us to filter out development events, but still use the same unity analytics dashboard.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        static void FilterAnalitics()
        {
            Debug.Log("Analytics: Sending developer filter event.");
            Analytics.CustomEvent("DevelopmentBuild", new Dictionary<string, object> {
                {"developer", Debug.isDebugBuild}
            });
        }
    }
}
#endif