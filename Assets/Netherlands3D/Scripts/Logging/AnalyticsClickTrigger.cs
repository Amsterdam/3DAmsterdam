using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Logging
{
    public class AnalyticsClickTrigger : MonoBehaviour, IPointerUpHandler
    {
		public void OnPointerUp(PointerEventData eventData)
		{
            var selectedName = this.gameObject.name;
            var containerName = (this.transform.parent) ? this.transform.parent.name : "";

            Analytics.SendEvent($"InterfaceItemClicked-{selectedName}",
                new Dictionary<string, object>
                {
                    { "Container",  containerName}
                }
            );
        }
    }
}