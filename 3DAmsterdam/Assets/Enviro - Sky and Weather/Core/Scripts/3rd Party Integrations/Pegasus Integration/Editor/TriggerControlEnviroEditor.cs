using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
#if ENVIRO_PEGASUS_SUPPORT
namespace Pegasus
{
    [CustomEditor(typeof(TriggerControlEnviro))]
    public class HeliosFadeEnviroEditor : Editor
    {
        GUIStyle m_boxStyle;
        GUIStyle m_wrapStyle;
        TriggerControlEnviro m_trigger;
		List<EnviroWeatherPreset> m_zoneWeatheraPrefabs = new List<EnviroWeatherPreset>();


        /// <summary>
        /// This is called when we select the poi in the editor
        /// </summary>
        private void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            m_trigger = (TriggerControlEnviro)target;

            if (m_trigger.m_enviroSky == null)
            {
                m_trigger.m_enviroSky = GameObject.FindObjectOfType<EnviroSky>();
            }

            if (m_trigger.m_enviroSky != null)
            {
                EnviroZone zone = m_trigger.m_enviroSky.gameObject.GetComponent<EnviroZone>();
                if (zone != null)
                {
					m_zoneWeatheraPrefabs = zone.zoneWeatherPresets;
                }
            }
        }

        /// <summary>
        /// Draw the gui
        /// </summary>
        public override void OnInspectorGUI()
        {
            //Get our trigger
            m_trigger = (TriggerControlEnviro)target;

            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box);
                m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_boxStyle.fontStyle = FontStyle.Bold;
                m_boxStyle.alignment = TextAnchor.UpperLeft;
            }

            //Setup the wrap style
            if (m_wrapStyle == null)
            {
                m_wrapStyle = new GUIStyle(GUI.skin.label);
                m_wrapStyle.wordWrap = true;
            }

            //Create a nice text intro
            GUILayout.BeginVertical("Enviro Weather Trigger", m_boxStyle);
            GUILayout.Space(20);
            EditorGUILayout.LabelField("This trigger controls Enviro time and weather settings.", m_wrapStyle);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(5);

            if (m_trigger.m_enviroSky == null)
            {
                EditorGUILayout.LabelField("Enviro Sky is missing from scene!!");
            }

            bool controlTime = EditorGUILayout.Toggle("Control Time", m_trigger.m_controlTime);
            float startTime = m_trigger.m_startTime;
            float endTime = m_trigger.m_endTime;

            if (controlTime)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(string.Format("{0:00}:{1:00} - {2:00}:{3:00}", Mathf.FloorToInt(startTime), (startTime % 1.0f) * 60f, Mathf.FloorToInt(endTime), (endTime % 1.0f) * 60f));
                startTime = EditorGUILayout.Slider(GetLabel("Start Time"), startTime, 0f, 23.99f);
                endTime = EditorGUILayout.Slider(GetLabel("End Time"), endTime, 0f, 23.99f);
                EditorGUI.indentLevel--;
            }

            bool controlWeather = EditorGUILayout.Toggle("Control Weather", m_trigger.m_controlWeather);
            int weatherID = m_trigger.m_weatherID;
            float weatherTransitionTime = m_trigger.m_weatherTransitionTime;

            if (controlWeather)
            {
                EditorGUI.indentLevel++;
                if (m_zoneWeatheraPrefabs.Count > 0)
                {
                    GUIContent[] zonePrefabs = new GUIContent[m_zoneWeatheraPrefabs.Count];
                    for (int idx = 0; idx < zonePrefabs.Length; idx++)
                    {
						zonePrefabs[idx] = new GUIContent(m_zoneWeatheraPrefabs[idx].Name);
                    }
                    weatherID = EditorGUILayout.Popup(GetLabel("Weather"), weatherID, zonePrefabs);
                }
                else
                {
                    weatherID = EditorGUILayout.IntField("Weather ID", weatherID);
                }
                weatherTransitionTime = EditorGUILayout.FloatField("Transition Time", weatherTransitionTime);
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(5);

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_trigger, "Made trigger changes");

                m_trigger.m_triggerAtStart = controlTime || controlWeather;
                m_trigger.m_triggerOnUpdate = controlTime;
                m_trigger.m_triggerAtEnd = false;

                m_trigger.m_controlTime = controlTime;
                m_trigger.m_startTime = startTime;
                m_trigger.m_endTime = endTime;

                m_trigger.m_controlWeather = controlWeather;
                m_trigger.m_weatherID = weatherID;
                m_trigger.m_weatherTransitionTime = weatherTransitionTime;

                //Mark it as dirty
                EditorUtility.SetDirty(m_trigger);
            }
        }

        /// <summary>
        /// Get a content label - look the tooltip up if possible
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private GUIContent GetLabel(string name)
        {
            string tooltip = "";
            if (m_tooltips.TryGetValue(name, out tooltip))
            {
                return new GUIContent(name, tooltip);
            }
            else
            {
                return new GUIContent(name);
            }
        }

        /// <summary>
        /// The tooltips
        /// </summary>
        private static Dictionary<string, string> m_tooltips = new Dictionary<string, string>
        {
            { "Min Height From", "Used to control how poi, lookat target and flythrough path heights are constrained. Manager - use the managers settings, collision - use whatever it collides with, terrain - use the terrain height, none - don't constrain." },
        };
    }
}
#endif