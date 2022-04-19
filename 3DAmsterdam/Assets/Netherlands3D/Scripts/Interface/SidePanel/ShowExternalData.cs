using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.Interface.SidePanel
{
    public class ShowExternalData : MonoBehaviour
    {
        [SerializeField]
        private ExternalData parsedJson;

        public void Load(string metaDataPath = "metadata.xml")
        {
            ServiceLocator.GetService<PropertiesPanel>().OpenObjectInformation("Laag informatie");
            ServiceLocator.GetService<PropertiesPanel>().AddLoadingSpinner();
            StartCoroutine(GetText(metaDataPath));
        }

        private IEnumerator GetText(string metaDataPath)
        {
            print("Load external data: " + Config.activeConfiguration.webserverRootPath + metaDataPath);
            UnityWebRequest www = UnityWebRequest.Get(Config.activeConfiguration.webserverRootPath + metaDataPath);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                ServiceLocator.GetService<PropertiesPanel>().ClearGeneratedFields();
                ServiceLocator.GetService<PropertiesPanel>().AddLabel("Sorry, laag metadata kan tijdelijk niet worden geladen.");
            }
            else
			{
				ServiceLocator.GetService<PropertiesPanel>().ClearGeneratedFields();
				parsedJson = JsonUtility.FromJson<ExternalData>(www.downloadHandler.text);
				DrawFields();
			}
		}

		private void DrawFields()
		{
			foreach (var field in parsedJson.fields)
			{
				switch (field.type)
				{
					case "title": 
                        ServiceLocator.GetService<PropertiesPanel>().AddTitle(field.content);
                        break;
					case "text":
                        ServiceLocator.GetService<PropertiesPanel>().AddTextfield(field.content);
                        break;
                    case "seperator_line":
                        ServiceLocator.GetService<PropertiesPanel>().AddSeperatorLine();
                        break;
                    case "label":
                        ServiceLocator.GetService<PropertiesPanel>().AddLabel(field.content);
                        break;
                    case "data_field":
                        ServiceLocator.GetService<PropertiesPanel>().AddDataField(field.key,field.content);
                        break;
                    case "link":
                        ServiceLocator.GetService<PropertiesPanel>().AddLink(field.key, field.content);
                        break;
                    case "spacer":
                        ServiceLocator.GetService<PropertiesPanel>().AddSpacer(field.space);
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