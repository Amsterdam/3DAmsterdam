using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class StringValueUnityEvent : UnityEvent<string> { }


[CreateAssetMenu(fileName = "StringEvent", menuName = "ScriptableObjects/Events/StringEvent", order = 0)]
[System.Serializable]
public class StringEvent : ScriptableObject
{
    public string eventName;
    public string description;
    public StringValueUnityEvent stringEvent;
}
