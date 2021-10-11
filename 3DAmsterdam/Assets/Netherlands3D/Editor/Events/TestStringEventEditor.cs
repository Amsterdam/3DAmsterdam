using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StringEvent))]
public class TestStringEventEditor : Editor
{
    private string dummyPayload = "";
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (StringEvent)target;

        if (Application.isPlaying)
        {
            dummyPayload = EditorGUILayout.TextField("Payload", dummyPayload);
            if (GUILayout.Button("Trigger", GUILayout.Height(40)))
            {
                script.unityEvent?.Invoke(dummyPayload);
            }
        }
    }
}