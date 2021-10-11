using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TriggerEvent))]
public class TestTriggerEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (TriggerEvent)target;

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Trigger", GUILayout.Height(40)))
            {
                script.unityEvent?.Invoke();
            }
        }
    }
}