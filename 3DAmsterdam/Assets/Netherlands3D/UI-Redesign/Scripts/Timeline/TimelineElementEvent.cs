using System;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Events
{

	[CreateAssetMenu(fileName = "TimelineElementEvent", menuName = "EventContainers/TimelineElementEvent", order = 0)]
	[System.Serializable]
	public class TimelineElementEvent : EventContainer<TimelineElement>
    {
		public override void InvokeStarted(TimelineElement timelineElementContent)
		{
            started.Invoke(timelineElementContent);
		}
	}
}