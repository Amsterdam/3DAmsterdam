using Netherlands3D.Interface.SidePanel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DisplayExternalData : MonoBehaviour
{
    [SerializeField]
    private ExternalData parsedJson;
    public void Load(string metaDataPath = "metadata.json")
    {
        //Open panel
        //UX - loading spinner

        StartCoroutine(GetText(metaDataPath));
    }

    private void Start()
    {
        var data = new ExternalData()
        {
            title = "Test",
            fields = new ExternalData.Field[] {
                    new ExternalData.Field(){ type = "title", content = "TITLE" },
                    new ExternalData.Field(){ type = "text", content = "texttexttexttexttexttexttexttext" },
                    new ExternalData.Field(){ type = "data_field", key = "key 1", content = "content 1" },
                    new ExternalData.Field(){ type = "data_field", key = "key 2", content = "content 2" },
                    new ExternalData.Field(){ type = "seperator_link", key = "key 2", content = "content 2" },
                    new ExternalData.Field(){ type = "link", key = "key link", content = "legit.link" },
                    new ExternalData.Field(){ type = "spacer" },
                    new ExternalData.Field(){ type = "data_field", key = "key 3", content = "content 3" }
                }
        };

        parsedJson = data;
        DrawFields();

    }

    private IEnumerator GetText(string metaDataPath)
    {
        if (true)
        {
            Debug.Log("HELLO");

            var data = new ExternalData()
            {
                title = "Test",
                fields = new ExternalData.Field[] {
                    new ExternalData.Field(){ type = "title", content = "TITLE" },
                    new ExternalData.Field(){ type = "text", content = "texttexttexttexttexttexttexttext" },
                    new ExternalData.Field(){ type = "data_field", key = "key 1", content = "content 1" },
                    new ExternalData.Field(){ type = "data_field", key = "key 2", content = "content 2" },
                    new ExternalData.Field(){ type = "seperator_link", key = "key 2", content = "content 2" },
                    new ExternalData.Field(){ type = "link", key = "key link", content = "legit.link" },
                    new ExternalData.Field(){ type = "spacer" },
                    new ExternalData.Field(){ type = "data_field", key = "key 3", content = "content 3" }
                }
            };

            parsedJson = data;
            DrawFields();
        }
        else
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
        
    }

    private void DrawFields()
    {
        foreach (var field in parsedJson.fields)
        {

            Debug.Log("DEBUG" + field.type);
            print("DEBUG" + field.type);

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
