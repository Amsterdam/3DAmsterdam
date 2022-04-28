using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericTextToFloatEvent : MonoBehaviour
{
    [SerializeField]
    private FloatEvent invokeFloatEvent;
    private bool listen = true;

    [SerializeField]
    private StringUnityEvent onReceivedEventFromOutside;

    private void Awake()
	{
        invokeFloatEvent.started.AddListener(CovertFloatToString);
    }

	private void CovertFloatToString(float value)
	{
        if (listen) onReceivedEventFromOutside.Invoke(value.ToString());
    }

	public void ConvertStringToFloatAndInvoke(string floatText)
    {
        if (float.TryParse(floatText, out float floatValue))
        {
            listen = false;
            invokeFloatEvent.Invoke(floatValue);
            listen = true;
        }
        else
        {
            Debug.Log($"Could not convert to float: {floatText}",this.gameObject);
        }
    }
}
