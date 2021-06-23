using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D
{
    public class AnalyticsTriggerInjector : MonoBehaviour
    {
        void Start()
        {
			//Start with adding events to the already existing Selectables in the scene
            var UISelectables = FindObjectsOfType<Selectable>(true);
            foreach (var selectable in UISelectables)
			{
				AddClickEventTrigger(selectable);
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