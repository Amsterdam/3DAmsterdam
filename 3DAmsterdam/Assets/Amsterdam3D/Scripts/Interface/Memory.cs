using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Memory : MonoBehaviour
{
	private Text memoryOutput;

	private void Awake()
	{
		memoryOutput = GetComponent<Text>();
	}

	IEnumerator Start()
	{
		while (true)
		{
			DrawMemoryUsageInHeap();
			yield return new WaitForSeconds(0.5f);
		}
	}

	private void DrawMemoryUsageInHeap(){
		memoryOutput.text = ConvertBytesToMegabytes(System.GC.GetTotalMemory(false)).ToString("F2") + "MB";
	}
	
    private double ConvertBytesToMegabytes(long bytes)
    {
        return (bytes / 1024f) / 1024f;
    }
}
