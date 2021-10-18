using UnityEngine;
using UnityEditor;
using System;

namespace Netherlands3D.Events
{
    public class EventsContainerWindow : EditorWindow
    {
        static string[] triggerEventContainers;
        static string[] stringEventContainers;
        static string[] vector3EventContainers;

        static string testData = "";
        static string filterName = "";

        static bool onlyShowEventsWithListeners = false;

        const string playerPrefsTestData = "EventsContainerTestData";

        Vector2 scrollPosition;
        // Add menu named "My Window" to the Window menu
        [MenuItem("Netherlands 3D/Event Containers Window")]
        static void Init()
        {
            EventsContainerWindow window = (EventsContainerWindow)EditorWindow.GetWindow(typeof(EventsContainerWindow), true, "Events Container Window");
            window.Show();
        }

		private void OnEnable()
		{
            testData = PlayerPrefs.GetString(playerPrefsTestData);
        }

		void OnGUI()
        {
            GUILayout.Label("Event test data", EditorStyles.boldLabel);

            //Store dummy data for our easy testing
            EditorGUI.BeginChangeCheck();
            testData = EditorGUILayout.TextField("", testData);
            if (EditorGUI.EndChangeCheck())
            {
                PlayerPrefs.SetString(playerPrefsTestData, testData);
            }

            GUILayout.Label("Filter", EditorStyles.boldLabel);
            filterName = EditorGUILayout.TextField("", filterName);

            scrollPosition = GUILayout.BeginScrollView(
                scrollPosition);

            //TriggerEvent
            GUILayout.Label("TriggerEvent Containers", EditorStyles.boldLabel);
            if (triggerEventContainers == null) triggerEventContainers = AssetDatabase.FindAssets("t:TriggerEvent");
            foreach (var triggerEventContainer in triggerEventContainers)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(triggerEventContainer);

                if (filterName.Length > 0 && assetPath.IndexOf(filterName, StringComparison.OrdinalIgnoreCase) == -1) continue;
                
                TriggerEvent asset = (TriggerEvent)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TriggerEvent));

                //if(asset.unityEvent.Get)

                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(asset, typeof(TriggerEvent), false);
                if (GUILayout.Button("Trigger", GUILayout.Height(20)))
                {
                    asset.unityEvent?.Invoke();
                }
                GUILayout.EndHorizontal();
            }

            //StringEvent
            GUILayout.Label("StringEvent Containers", EditorStyles.boldLabel);
            if (stringEventContainers == null) stringEventContainers = AssetDatabase.FindAssets("t:StringEvent");
            foreach (var stringEventContainer in stringEventContainers)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(stringEventContainer);

                if (filterName.Length > 0 && assetPath.IndexOf(filterName, StringComparison.OrdinalIgnoreCase) == -1) continue;

                StringEvent asset = (StringEvent)AssetDatabase.LoadAssetAtPath(assetPath, typeof(StringEvent));
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(asset, typeof(StringEvent), false);
                if (GUILayout.Button("Trigger", GUILayout.Height(20)))
                {
                    asset.unityEvent?.Invoke(testData);
                }
                GUILayout.EndHorizontal();
            }

            //Vector3Event
            GUILayout.Label("Vector3Event Containers", EditorStyles.boldLabel);
            if (vector3EventContainers == null) vector3EventContainers = AssetDatabase.FindAssets("t:Vector3Event");
            foreach (var vector3EventContainer in vector3EventContainers)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(vector3EventContainer);

                if (filterName.Length > 0 && assetPath.IndexOf(filterName, StringComparison.OrdinalIgnoreCase) == -1) continue;

                Vector3Event asset = (Vector3Event)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Vector3Event));
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(asset, typeof(Vector3Event), false);
                if (GUILayout.Button("Trigger", GUILayout.Height(20)))
                {
                    var vector3string = testData.Split(',');
                    Vector3 vector3 = new Vector3(
                        float.Parse(vector3string[0]),
                        float.Parse(vector3string[1]),
                        float.Parse(vector3string[2])
                    );
                    asset.unityEvent?.Invoke(vector3);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}