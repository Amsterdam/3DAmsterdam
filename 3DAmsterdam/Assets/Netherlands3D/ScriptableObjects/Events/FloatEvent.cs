using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FloatValueUnityEvent : UnityEvent<string> { }

[CreateAssetMenu(fileName = "FloatEvent", menuName = "ScriptableObjects/Events/FloatEvent", order = 0)]
[System.Serializable]
public class FloatEvent : ScriptableObjectEvent
{
    public FloatValueUnityEvent floatEvent;
}

