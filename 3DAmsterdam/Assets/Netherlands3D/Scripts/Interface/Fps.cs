using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Netherlands3D.Logging;
using TMPro;
namespace Netherlands3D.Interface
{
	public class Fps : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI fpsCounter;

		[SerializeField]
		private Image fpsBackground;

		private int framesVisualFPS = 0;
		private double lastInterval = 0;

		[SerializeField]
		private bool automaticFpsLogging = false;

		private float updateAnalyticsInterval = 10.0f; //Every # seconds we log our average fps to the analytics
		
		private int framesAnalytics = 0;
		private double lastIntervalAnalytics = 0;

		private int minimumFramesRenderedBeforeLogging = 30;

		private int analyticsFpsGroupSize = 5; //The average framerate analytics are grouped in groups with this size. A value of 5 would give groups 5,10,15, and up

		private bool enabledVisualFPS = false;

		private float timeNow = 0;

		[Header("FPS Numbers on screen")]
		[SerializeField]
		private int badFpsThreshold = 10;
		private int goodFpsThreshold = 30;
		[SerializeField]
		private float updateInterval = 0.5f;

		private bool applicationIsActive = true;

		public static int fpsLogGroup = 0;

		private void Awake()
		{
			ToggleVisualFPS(false);

			framesVisualFPS = 0;
			lastInterval = Time.realtimeSinceStartup;

			lastIntervalAnalytics = Time.realtimeSinceStartup;
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

			//Determine framerate group after we have been running for some frames, and the app is in the foreground
			if(applicationIsActive && Time.frameCount > minimumFramesRenderedBeforeLogging)
			{
				++framesAnalytics;
				if (timeNow > lastIntervalAnalytics + updateAnalyticsInterval)
				{
					DetermineFpsGroup((float)(framesAnalytics / (timeNow - lastIntervalAnalytics)));
					framesAnalytics = 0;
					lastIntervalAnalytics = timeNow;
				}
			}
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
		/// Calculate the average framerate group. Its rounded up into to FPS groups.
		/// </summary>
		/// <param name="fps">The avarage framerate count at this time of logging</param>
		private void DetermineFpsGroup(float fps)
		{
			//Determine fps log group variable. This can be used to determine performance, or show in events.
			fpsLogGroup = Mathf.Clamp(Mathf.RoundToInt(Mathf.Round(fps / analyticsFpsGroupSize) * analyticsFpsGroupSize), analyticsFpsGroupSize, 200);

			//Optionaly send this average framerate tick as an event every # seconds
			if(automaticFpsLogging)
				Analytics.SendEvent("FPS",$"{fpsLogGroup}",$"{fps}");
		}

		/// <summary>
		/// Method we call from javascript, telling unity if the tab/application is active.
		/// Browsers throttle down applications in background tabs (to 1 fps) so we want to ignore those fps counts.
		/// </summary>
		/// <param name="isActive">Is the application active in the foreground, running at max performance, this should be 1, else 0.</param>
		public void ActiveApplication(float isActive)
		{
			bool active = (isActive == 1); //We convert a number to a bool ( SendMessage from javascript only supports strings and numbers ) 
			Debug.Log("Application is on foreground: " + active);
			applicationIsActive = active;

			//Reset coundown before logging resumes
			if(applicationIsActive) 
			{
				framesAnalytics = 0;
				lastIntervalAnalytics = timeNow;
			}
		}
	}
}