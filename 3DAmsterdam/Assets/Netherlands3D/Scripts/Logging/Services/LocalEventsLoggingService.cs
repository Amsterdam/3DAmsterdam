using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Netherlands3D.Logging.Services
{
	/// <summary>
	/// This class is an example of how to create your own Analytics Service
	/// In this case we write the received lines to a local text file which would not work on WebGL
	/// </summary>
	public class LocalEventsLoggingService : AnalyticsService
	{
		private string targetFile = "";

		private void Awake()
		{
#if UNITY_EDITOR
			//Create a fresh new local logfile in our datapath with a random name next to our project Assets folder
			targetFile = Application.dataPath + string.Format(@"/../Logs/eventsLogFile-{0}.txt", Guid.NewGuid());
			Debug.Log($"Writing Analytics events to local file: {targetFile}");
#endif
		}

		public override void SendEvent(string category, string action, string label = "")
		{
#if UNITY_EDITOR
			Debug.Log($"Wrote analytics event to {targetFile}");

			//Append the events as lines to our text file
			StreamWriter writer = new StreamWriter(targetFile, true);
			writer.WriteLine($"[{category}] [{action}] [{label}]");
			writer.WriteLine($"Time: { DateTime.Now }");
			writer.WriteLine("");
			writer.Close();
#endif
		}
	}
}
