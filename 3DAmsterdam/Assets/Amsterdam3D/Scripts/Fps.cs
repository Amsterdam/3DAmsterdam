using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Amsterdam3D.Debug
{
	public class Fps : MonoBehaviour
	{
		private Text fpsCounter;
		private float count;

		private void Awake()
		{
			fpsCounter = GetComponent<Text>();
		}

		IEnumerator Start()
		{
			while (true)
			{
				DrawFps();
				yield return new WaitForSeconds(0.5f);
			}
		}

		private void DrawFps()
		{
			count = (1 / Time.deltaTime);
			fpsCounter.text = "FPS:" + (Mathf.Round(count));
		}
	}
}