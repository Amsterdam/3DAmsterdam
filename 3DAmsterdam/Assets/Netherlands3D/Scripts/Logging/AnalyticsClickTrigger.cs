using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.Logging
{
    public class AnalyticsClickTrigger : MonoBehaviour, IPointerUpHandler
    {
        public string category;
        public string action;
        public string label;
		public void OnPointerUp(PointerEventData eventData)
		{
            var selectedName = this.gameObject.name;
            var containerName = (this.transform.parent) ? this.transform.parent.name : "Root";

            //Optionaly send the state of a toggle (on/off) after we clicked it
            var toggle = GetComponent<Toggle>();
            if(toggle && (!toggle.group || (toggle.group && toggle.group.allowSwitchOff)))
            {
                //Togle isOn value is set after pointer up. So we read the opposite to log on/off:
                Analytics.SendEvent(category, action, (toggle.isOn) ? "Off" : "On");
			}
            else{
                Analytics.SendEvent(category,action,label);
            }
        }
    }
}