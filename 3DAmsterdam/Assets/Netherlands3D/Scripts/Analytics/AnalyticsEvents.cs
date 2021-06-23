using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace Netherlands3D
{
    public class AnalyticsEvents
    {
        /// <summary>
        /// Own custom events wrapper. This allows us to easily swap the Unity analytics with something else, for example Google Analytics
        /// </summary>
        /// <param name="eventName">Main name of the event</param>
        /// <param name="eventData">Event data</param>
		public static void CustomEvent(string eventName, Dictionary<string,object> eventData)
        {
#if !UNITY_EDITOR
            //Only logging events if we are not in the editor, and are in production/live
            Debug.Log($"Netherlands3D Analytics event: {eventName}");
            Analytics.CustomEvent(eventName, eventData);
            Analytics.FlushEvents();
            return;
#endif
            Debug.Log($"<color=#61A196>[Analytics Event]  {eventName} (Not sent in Editor)</color>");
            foreach(KeyValuePair<string,object> keyValue in eventData)
            {
                Debug.Log($"<color=#61A146>[Event data] {keyValue.Key} : {keyValue.Value}</color>");
            }
        }
    }
}