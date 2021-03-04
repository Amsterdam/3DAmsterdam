using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Analytics;
using System.Collections.Generic;

namespace Amsterdam3D.Interface
{
	public class Fps : MonoBehaviour
	{
		[SerializeField]
		private Text fpsCounter;

		[SerializeField]
		private Image fpsBackground;

		[SerializeField]
		private bool logFpsGroupsToAnalytics = true;

		[SerializeField]
		private float updateAnalyticsInterval = 10.0f; //Every # seconds we log our average fps to the analytics

		private double lastInterval;
		private double lastIntervalAnalytics;

		private int framesVisualFPS = 0;
		private int framesAnalytics = 0;

		private bool enabledVisualFPS = false;

		private float timeNow = 0;

		[SerializeField]
		[Tooltip("The average framerate analytics are grouped in groups with this size. A value of 5 would give groups 5,10,15, and up")]
		private int analyticsFpsGroupSize = 5;

		[Header("FPS Numbers on screen")]
		[SerializeField]
		private int badFpsThreshold = 10;
		private int goodFpsThreshold = 30;
		[SerializeField]
		private float updateInterval = 0.5f;

		private void Awake()
		{
			ToggleVisualFPS(false);

			lastInterval = Time.realtimeSinceStartup;
			lastIntervalAnalytics = Time.realtimeSinceStartup;

			framesVisualFPS = 0;
			framesAnalytics = 0;
		}

		private void Update()
		{
			CalculateAverageFPS();
		}

		/// <summary>
		/// Shows or hides the visual FPS number in the screen
		/// </summary>
		/// <param name="enabled"></param>
		public void ToggleVisualFPS(bool enabled)
		{
			enabledVisualFPS = enabled;

			fpsCounter.enabled = enabledVisualFPS;
			fpsBackground.enabled = enabledVisualFPS;
		}

		/// <summary>
		/// Calculates the average frames drawn per second (updates) and optionaly shows/logs it
		/// </summary>
		private void CalculateAverageFPS()
		{
			timeNow = Time.realtimeSinceStartup;

			if (enabledVisualFPS)
			{
				++framesVisualFPS;
				if (timeNow > lastInterval + updateInterval)
				{
					DrawFps((float)(framesVisualFPS / (timeNow - lastInterval)));
					framesVisualFPS = 0;
					lastInterval = timeNow;
				}
			}

			#if !UNITY_EDITOR
			if(logFpsGroupsToAnalytics)
			{
				++framesAnalytics;
				if (timeNow > lastIntervalAnalytics + updateAnalyticsInterval)
				{
					LogFPS((float)(framesAnalytics / (timeNow - lastIntervalAnalytics)));
					framesAnalytics = 0;
					lastIntervalAnalytics = timeNow;
				}
			}
			#endif
		}

		/// <summary>
		/// Updates the visual fps counter text interpolated color red->green based on fps
		/// </summary>
		/// <param name="fps">The avarage framerate count to draw</param>
		private void DrawFps(float fps)
		{
			fpsCounter.text = Mathf.Round(fps).ToString();
			fpsCounter.color = Color.Lerp(Color.red, Color.green, Mathf.InverseLerp(badFpsThreshold, goodFpsThreshold, fps));
		}
		
		/// <summary>
		/// Logs the FPS to Unity Analytics. Its rounded up into to FPS groups.
		/// </summary>
		/// <param name="fps">The avarage framerate count at this time of logging</param>
		private void LogFPS(float fps)
		{
			int fpsLogGroup = Mathf.Clamp(Mathf.RoundToInt(Mathf.Round(fps / analyticsFpsGroupSize) * analyticsFpsGroupSize), analyticsFpsGroupSize, 200);			
			Debug.Log("Analytics: fpsGroup " + fpsLogGroup);
			Analytics.CustomEvent("FPS",
			new Dictionary<string, object>
			  {
				{ "fpsGroup", fpsLogGroup },
				{ "fps", fps }
			  });
		}
	}
}