using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.WebGL
{
    public class DetermineJSONType : MonoBehaviour
    {
        [System.Serializable]
        public struct JSONTypeEvent{
            public string typeDescription;
            public string contentSubstring;
            public StringEvent onSubstringFoundEvent;
		}

        private static readonly Regex removeWhiteSpace = new Regex(@"\s+");

        [SerializeField]
        private JSONTypeEvent[] jsonTypeEvents;

        [SerializeField]
        private int streamReadFirstCharacters = 100;

        /// <summary>
        /// Checks a comma seperated list of imported files
        /// if it contains a .json file extention and determines its content type
        /// </summary>
        /// <param name="importedFiles"></param>
        public void CheckImportedFiles(string importedFiles)
        {
            string[] files = importedFiles.Split(',');
            foreach(var filename in files)
            {
                if(filename.EndsWith(".json") || filename.EndsWith(".geojson"))
				{
					DetermineJSONContent(filename);
				}
			}
        }

		private void DetermineJSONContent(string file)
		{
            Debug.Log("This appears to be a json file. Determining type by looking for substrings:");
			var filePath = file;
			if (!Path.IsPathRooted(filePath))
			{
				Debug.Log($"{filePath} is relative. Appended persistentDataPath.");
				filePath = Application.persistentDataPath + "/" + file;
			}

			if (!File.Exists(filePath))
			{
				Debug.Log($"{filePath} not found");
				return;
			}

            //StreamRead first part of json content to look for substrings
            var characters = new string(ReadCharacters(filePath,streamReadFirstCharacters));
            removeWhiteSpace.Replace(characters, "");
            foreach(var jsonType in jsonTypeEvents)
            {
                if (characters.Contains(jsonType.contentSubstring))
                {
                    Debug.Log(jsonType.typeDescription);
                    jsonType.onSubstringFoundEvent.started.Invoke(file);
                    return;
				}
			}
        }

        public char[] ReadCharacters(string filename, int count)
        {
            using (var stream = File.OpenRead(filename))
            {
                var maxCharacters = Mathf.Min(count, (int)stream.Length);
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    char[] buffer = new char[maxCharacters];
                    int n = reader.ReadBlock(buffer, 0, count);
                    char[] result = new char[n];
                    Array.Copy(buffer, result, n);
                    return result;
                }
            }
        }
    }
}