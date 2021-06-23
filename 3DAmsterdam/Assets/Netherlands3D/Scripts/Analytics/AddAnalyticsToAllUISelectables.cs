using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D
{
    public class AddAnalyticsToAllUISelectables : MonoBehaviour
    {
        void Start()
        {
            var UISelectables = FindObjectsOfType<Selectable>(true);
            foreach (var selectable in UISelectables)
			{
				AddPointerDownEventTrigger(selectable);
			}
		}

		private void AddPointerDownEventTrigger(Selectable selectable)
		{
			//Add EventTrigger if it does not exist yet on this selectable
			EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
			if(!trigger)
				trigger = selectable.gameObject.AddComponent<EventTrigger>();

			//Add a new entry catching a click
			var pointerDown = new EventTrigger.Entry();
			pointerDown.eventID = EventTriggerType.PointerUp;
			pointerDown.callback.AddListener((pointerEvent) => SelectableClicked(pointerEvent, selectable));

			//Add this entry to our trigger
			trigger.triggers.Add(pointerDown);
		}

		private void SelectableClicked(BaseEventData pointerEvent, Selectable selectable)
		{
			var selectedName = selectable.name;
			var containerName = (selectable.transform.parent) ? selectable.transform.parent.name : "";

			AnalyticsEvents.CustomEvent("InterfaceItemClicked",
                new Dictionary<string, object>
                {
                    { "Name", selectedName },
                    { "Container",  containerName}
                }
            );
        }
    }
}