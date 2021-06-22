using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D
{
    public class Analytics
    {
        /// <summary>
        /// Own custom events wrapper. This allows us to easily swap the Unity analytics with something else, for example Google Analytics
        /// </summary>
        /// <param name="eventName">Main name of the event</param>
        /// <param name="eventData">Event data</param>
		public static void CustomEvent(string eventName, Dictionary<string,object> eventData)
        {
            Debug.Log($"Netherlands3D Analytics event: {eventName}");
#if !UNITY_EDITOR && PRODUCTION
            //Only logging events if we are not in the editor, and are in production/live
            UnityEngine.Analytics.AnalyticsEvent.Custom(eventName, eventData);
#endif
        }
    }
}