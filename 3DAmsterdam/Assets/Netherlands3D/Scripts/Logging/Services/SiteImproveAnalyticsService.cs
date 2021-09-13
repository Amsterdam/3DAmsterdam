using Netherlands3D.Interface;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
//using UnityEngine.Analytics;

//Siteimprove.com used by Amsterdam as a Analytics Service
namespace Netherlands3D.Logging.Services
{
	public class SiteImproveAnalyticsService : AnalyticsService
	{
		[SerializeField]
		private VisualStatOverlay visualStatOverlay;

		[DllImport("__Internal")]
		private static extern void PushEvent(string category = "category", string action = "action", string label = "label");

		public override void SendEvent(string category, string action, string label = "")
		{		
#if !UNITY_EDITOR && UNITY_WEBGL
			PushEvent(category, action, label);
#endif
		}

		public void DisplayStatsHeatMap(string urlToCSV)
		{
			StartCoroutine(GetCSVStats(urlToCSV));
		}

#if UNITY_EDITOR
		[ContextMenu("Load CSV statistic file")]
		public void DisplayStatsHeatMapEditor()
		{
			if (Application.isPlaying)
			{
				string path = EditorUtility.OpenFilePanel("Open SiteImprove analytics csv file", "", "csv");
				StartCoroutine(GetCSVStats($"file:///{path}"));
			}
			else
			{
				Debug.Log("Make sure the editor is playing");
			}
		}
#endif
		private void InjectStatisticVisual(AnalyticsClickTrigger targetEventTriggerObject, string percentageOfTotal, string visitsWithThisEvent)
		{
			VisualStatOverlay statOverlay = Instantiate(visualStatOverlay, FindObjectOfType<Canvas>().transform);
			statOverlay.transform.position = targetEventTriggerObject.transform.position;
			statOverlay.SetParameters(
				targetEventTriggerObject.transform as RectTransform,
				percentageOfTotal.Replace("%","").Trim(),
				visitsWithThisEvent.Trim()
			);
		}

		IEnumerator GetCSVStats(string urlToCSV)
		{
			Debug.Log($"Loading: {urlToCSV}");
			UnityWebRequest www = UnityWebRequest.Get(urlToCSV);
			yield return www.SendWebRequest();

			if (www.result != UnityWebRequest.Result.Success)
			{
				Debug.Log(www.error);
			}
			else
			{
				var stringToRead = Encoding.Unicode.GetString(www.downloadHandler.data);
				ReadAnalyticsData(stringToRead);
			}
		}

		public void ReadAnalyticsData(string stringToRead)
		{
			AnalyticsClickTrigger[] clickTriggers = this.transform.parent.GetComponentsInChildren<AnalyticsClickTrigger>(true);
			StringReader strReader = new StringReader(stringToRead);
			while (true)
			{
				var csvLine = strReader.ReadLine();
				if (csvLine != null)
				{
					if (csvLine.Contains("\"") && !csvLine.Contains("Performance") && !csvLine.Contains("Categorie"))
					{
						//Data line (filtered out performance)
						string[] fields = csvLine.Split('\t');
						var actionObject = fields[1].Replace("\"", "");
						var withParentObject = fields[0].Replace("\"", "");
						var percentageOfTotal = fields[4].Replace("\"", "");
						var visitsWithThisEvent = fields[5].Replace("\"", "");

						foreach (var clickTrigger in clickTriggers)
						{
							Debug.Log($"{clickTrigger.name} == {actionObject} && {clickTrigger.transform.parent.name} == {withParentObject}");
							if (clickTrigger.name == actionObject && clickTrigger.transform.parent.name == withParentObject)
							{
								InjectStatisticVisual(clickTrigger, percentageOfTotal, visitsWithThisEvent);
							}
						}
					}
				}
				else
				{
					strReader.Close();
					return;
				}
			}
		}
	}
}