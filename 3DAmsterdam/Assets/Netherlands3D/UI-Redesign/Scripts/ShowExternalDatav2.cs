using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.Interface.SidePanel
{
    public class ShowExternalDatav2 : MonoBehaviour
    {
        [SerializeField]
        private ExternalData parsedJson;

        public void Load(string metaDataPath = "metadata.json")
        {
            //SideTabPanel.Instance.OpenObjectInformation();
            PropertiesPanel.Instance.AddLoadingSpinner();
            StartCoroutine(GetText(metaDataPath));
        }

        private IEnumerator GetText(string metaDataPath)
        {
            print("Load external data: " + metaDataPath);
            UnityWebRequest www = UnityWebRequest.Get(metaDataPath);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                GenerateComponents.Instance.ClearGeneratedFields();
                GenerateComponents.Instance.AddLabel("Sorry, de bron informatie van deze laag kan tijdelijk niet worden geladen.");
            }
            else
            {
                GenerateComponents.Instance.ClearGeneratedFields();
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
                        GenerateComponents.Instance.AddTitle(field.content);
                        break;
                    case "text":
                        GenerateComponents.Instance.AddTextfield(field.content);
                        break;
                    case "seperator_line":
                        GenerateComponents.Instance.AddSeperatorLine();
                        break;
                    case "label":
                        GenerateComponents.Instance.AddLabel(field.content);
                        break;
                    case "data_field":
                        GenerateComponents.Instance.AddDataField(field.key, field.content);
                        break;
                    case "link":
                        GenerateComponents.Instance.AddLink(field.key, field.content);
                        break;
                    case "spacer":
                        GenerateComponents.Instance.AddSpacer(field.space);
                        break;
                }
            }
        }
    }
}