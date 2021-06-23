using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D
{
    public class AnalyticsClickTrigger : MonoBehaviour, IPointerUpHandler
    {
		public void OnPointerUp(PointerEventData eventData)
		{
            var selectedName = this.gameObject.name;
            var containerName = (this.transform.parent) ? this.transform.parent.name : "";

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