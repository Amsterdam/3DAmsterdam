using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimelineElementEventListener : MonoBehaviour
{
    [SerializeField]
    private TimelineElementEvent onEvent;

    [SerializeField]
    private UnityEvent<TimelineElement> trigger;

    void Awake()
    {
        if (onEvent)
        {
            onEvent.AddListenerStarted(Invoke);
        }
    }

    public void Invoke(TimelineElement value)
	{
        trigger.Invoke(value);
    }
}
