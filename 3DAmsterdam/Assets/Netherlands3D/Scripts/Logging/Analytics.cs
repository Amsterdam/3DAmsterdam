using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Netherlands3D.Logging.Services;
using Netherlands3D.Interface;

namespace Netherlands3D.Logging
{
	public class Analytics : MonoBehaviour
    {
		[Header("Add own custom services using the AnalyticsService base class")]

		[SerializeField]
		private AnalyticsService[] analyticsServices;
		public static Analytics Instance;

		[SerializeField]
		private bool logInConsole = true;

		void Awake()
        {
			Instance = this;
			//Start with adding events to the already existing Selectables in the scene
			var UISelectables = FindObjectsOfType<Selectable>(true);
            foreach (var selectable in UISelectables)
			{
				AddClickEventTrigger(selectable);
			}

#if UNITY_EDITOR || !PRODUCTION
			//Start with a first log stating if this is a dev build or in editor
			//so we can filter out Production results in the Unity DashBoard
			SendEvent("Build", "Started Development build");
#endif
		}

		/// <summary>
		/// Send an event down to our list of analytics services
		/// </summary>
		/// <param name="eventName">Main name of the event</param>
		/// <param name="eventData">Event data with field names and their values</param>
		public static void SendEvent(string category, string action, string label = "")
		{
			#if UNITY_EDITOR
			//Show our events in the console
			if(Instance.logInConsole)
			{
				Debug.Log($"<color={ConsoleColors.EventHexColor}><b>[Analytics Event]</b> {category},{action},{label}</color>");
				if(Fps.fpsLogGroup != 0)
					Debug.Log($"<color={ConsoleColors.EventHexColor}><b>[Analytics Performance Event]</b> Fps group:{Fps.fpsLogGroup}</color>");
			}
			#endif

			//Send the event down to our analytics service(s)
			foreach (var service in Instance.analyticsServices)
			{
				if (service.enabled)
				{
					service.SendEvent(category, action, label);

					//We send average framerate after every action, as fps groups
					if(Fps.fpsLogGroup != 0)
						service.SendEvent("Performance", "FPS", $"{Fps.fpsLogGroup}");
				}
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