using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Netherlands3D.Logging.Services;

namespace Netherlands3D.Logging
{
	public class Analytics : MonoBehaviour, IUniqueService
    {
		[Header("You can replace Unity Analytics, or add more services using the AnalyticsService base class")]

		[SerializeField]
		private AnalyticsService[] analyticsServices;

		[SerializeField]
		private bool logInConsole = true;

		void Awake()
        {
			//Start with adding events to the already existing Selectables in the scene
			var UISelectables = FindObjectsOfType<Selectable>(true);
            foreach (var selectable in UISelectables)
			{
				AddClickEventTrigger(selectable);
			}

#if UNITY_EDITOR || !PRODUCTION
			//Start with a first log stating if this is a dev build or in editor
			//so we can filter out Production results in the Unity DashBoard
			SendEvent("DevelopmentBuild", new Dictionary<string, object> {
				{"developer", Debug.isDebugBuild}
			});
#endif
		}

		/// <summary>
		/// Send an event down to our list of analytics services
		/// </summary>
		/// <param name="eventName">Main name of the event</param>
		/// <param name="eventData">Event data with field names and their values</param>
		public static void SendEvent(string eventName, Dictionary<string, object> eventData)
		{
			//Show our events in the console
			if(ServiceLocator.GetService<Analytics>().logInConsole)
			{
				Debug.Log($"<color={ConsoleColors.EventHexColor}>[Analytics Event]  {eventName} (Not sent in Editor)</color>");
				foreach (KeyValuePair<string, object> keyValue in eventData)
				{
					Debug.Log($"<color={ConsoleColors.EventDataHexColor}>[Event data] {keyValue.Key} : {keyValue.Value}</color>");
				}
			}

			//Send the event down to our analytics service(s)
			foreach (var service in ServiceLocator.GetService<Analytics>().analyticsServices)
			{
				if(service.enabled)	service.SendEvent(eventName, eventData);
			}			
		}

		//Dynamicly spawned UI items/menu's can receive listeners via their container
		public static void AddToContainer(GameObject specificTargetContainer)
		{
			var generatedUISelectables = specificTargetContainer.GetComponentsInChildren<Selectable>();
			foreach (var selectable in generatedUISelectables)
			{
				AddClickEventTrigger(selectable);
			}
		}

		private static void AddClickEventTrigger(Selectable selectable)
		{
			AnalyticsClickTrigger clickTrigger = selectable.GetComponent<AnalyticsClickTrigger>();
			if (!clickTrigger)	selectable.gameObject.AddComponent<AnalyticsClickTrigger>();
		}
    }
}