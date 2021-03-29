using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.Interface.SidePanel
{
    public class ShowExternalData : MonoBehaviour
    {
        public void Load(string metaDataPath = "metadata.xml")
        {
            PropertiesPanel.Instance.OpenObjectInformation("Laag informatie");
            PropertiesPanel.Instance.AddLoadingSpinner();
            StartCoroutine(GetText(metaDataPath));
        }

        private IEnumerator GetText(string metaDataPath)
        {
            print("Load external data: " + Config.activeConfiguration.webserverRootPath + "/" + metaDataPath);
            UnityWebRequest www = UnityWebRequest.Get(Config.activeConfiguration.webserverRootPath + "/" + metaDataPath);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
			{
				PropertiesPanel.Instance.ClearGeneratedFields();
				var parsedJson = JsonUtility.FromJson<ExternalData>(www.downloadHandler.text);
				DrawFields(parsedJson);
			}
		}

		private static void DrawFields(ExternalData parsedJson)
		{
			foreach (var field in parsedJson.fields)
			{
				switch (field.type)
				{
					case "title": 
                        PropertiesPanel.Instance.AddTitle(field.content);
                        break;
					case "text":
                        PropertiesPanel.Instance.AddTextfield(field.content);
                        break;
                    case "seperator_line":
                        PropertiesPanel.Instance.AddSeperatorLine();
                        break;
                    case "label":
                        PropertiesPanel.Instance.AddLabel(field.content);
                        break;
                    case "data_field":
                        PropertiesPanel.Instance.AddDataField(field.key,field.content);
                        break;
                    case "link":
                        PropertiesPanel.Instance.AddLink(field.key, field.content);
                        break;
                    case "spacer":
                        PropertiesPanel.Instance.AddSpacer(field.space);
                        break;
				}
			}
		}
	}

    [System.Serializable]
    public class ExternalData
    {
        public string title = "";
        public Field[] fields;

        [System.Serializable]
        public class Field
        {
            public string type = "title";
            public string key = "";
            public string content = "";
            public float space = 5.0f;
        }
    }
}