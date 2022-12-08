using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Netherlands3D.Interface
{
	public class Memory : MonoBehaviour
	{
		private TextMeshProUGUI memoryOutput;

		private void Awake()
		{
			memoryOutput = GetComponent<TextMeshProUGUI>();
		}

		private void OnEnable()
		{
			StartCoroutine(MemoryTick());
		}
		private void OnDisable()
		{
			StopAllCoroutines();
		}

		IEnumerator MemoryTick()
		{
			while (true)
			{
				DrawMemoryUsageInHeap();
				yield return new WaitForSeconds(0.5f);
			}
		}

		/// <summary>
		/// Draws the GC total memory as MB in the Text component
		/// </summary>
		private void DrawMemoryUsageInHeap()
		{
			memoryOutput.text = $"Heap size: {SystemInfo.systemMemorySize}MB \nGC: {ConvertBytesToMegabytes(System.GC.GetTotalMemory(false)):F2}MB";
		}

		/// <summary>
		/// Convert bytes to megabytes (easier to read)
		/// </summary>
		/// <param name="bytes">Number of bytes</param>
		/// <returns></returns>
		private double ConvertBytesToMegabytes(long bytes)
		{
			return (bytes / 1024f) / 1024f;
		}
	}
}