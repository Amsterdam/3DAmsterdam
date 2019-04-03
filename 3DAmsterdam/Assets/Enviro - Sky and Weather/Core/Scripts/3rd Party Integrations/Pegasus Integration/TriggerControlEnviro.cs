using System.Collections;
using UnityEngine;
#if ENVIRO_PEGASUS_SUPPORT
namespace Pegasus
{
    /// <summary>
    /// Control the weather in Enviro from Pegasus
    /// </summary>
    public class TriggerControlEnviro : TriggerBase
    {
        public EnviroSkyMgr m_enviroSky;

        public bool m_controlTime = false;
        public float m_startTime = 0f;
        public float m_endTime = 23.999f;

        public bool m_controlWeather = false;
        public int m_weatherID = 0;
        public float m_weatherTransitionTime = 10f; //Seconds

        /// <summary>
        /// Called when the trigger starts
        /// </summary>
        /// <param name="poi"></param>
        public override void OnStart(PegasusPoi poi)
        {
            if (poi == null)
            {
                Debug.LogWarning(string.Format("Poi was not supplied on {0} - exiting", name));
                return;
            }

            if (m_enviroSky == null)
            {
                m_enviroSky = GameObject.FindObjectOfType<EnviroSkyMgr>();
            }

            if (m_enviroSky == null)
            {
                Debug.LogWarning(string.Format("EnviroSky Manager was not located on {0} - exiting", name));
                return;
            }

            if (m_controlTime)
            {
                m_enviroSky.SetTimeOfDay(m_startTime);
            }

            if (m_controlWeather)
            {
				m_enviroSky.EnviroWeatherSettings.effectTransitionSpeed = m_weatherTransitionTime;
                m_enviroSky.ChangeWeather(m_weatherID);
            }
        }

        /// <summary>
        /// Called when the trigger is updated
        /// </summary>
        /// <param name="poi"></param>
        public override void OnUpdate(PegasusPoi poi, float progress)
        {
            if (m_enviroSky == null)
            {
                return;
            }

            if (m_controlTime)
            {
                m_enviroSky.SetTimeOfDay(m_startTime + ((m_endTime - m_startTime) * progress));
            }
        }
    }
}
#endif