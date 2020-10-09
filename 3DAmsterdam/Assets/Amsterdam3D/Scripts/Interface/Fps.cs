using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public class Fps : MonoBehaviour
	{
		private Text fpsCounter;

		public float updateInterval = 0.5f;
		private double lastInterval;
		private int frames = 0;

		private void Awake()
		{
			fpsCounter = GetComponent<Text>();
			lastInterval = Time.realtimeSinceStartup;
			frames = 0;
		}

		private void Update()
		{
			CalculateAverageFps();
		}

		private void CalculateAverageFps()
		{
			++frames;
			var timeNow = Time.realtimeSinceStartup;
			if (timeNow > lastInterval + updateInterval)
			{
				DrawFps((float)(frames / (timeNow - lastInterval)));
				frames = 0;
				lastInterval = timeNow;
			}
		}

		private void DrawFps(float fps)
		{
			fpsCounter.text = Mathf.Round(fps).ToString();
		}
	}
}