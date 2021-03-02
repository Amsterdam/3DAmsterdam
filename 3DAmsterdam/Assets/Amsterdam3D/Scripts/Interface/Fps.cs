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
		private float updateInterval = 0.5f;

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

		private int analyticsFpsGroupSize = 5; //5, 15, 15, 20 fps groups etc.etc.

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

		public void ToggleVisualFPS(bool enabled)
		{
			enabledVisualFPS = enabled;

			fpsCounter.enabled = enabledVisualFPS;
			fpsBackground.enabled = enabledVisualFPS;
		}

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
		}

		private void DrawFps(float fps)
		{
			fpsCounter.text = Mathf.Round(fps).ToString();
			fpsCounter.color = Color.Lerp(Color.red, Color.green, Mathf.InverseLerp(10, 30, fps));
		}
		
		private void LogFPS(float fps)
		{
			int fpsLogGroup = Mathf.RoundToInt(Mathf.Round(fps / analyticsFpsGroupSize) * analyticsFpsGroupSize);
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