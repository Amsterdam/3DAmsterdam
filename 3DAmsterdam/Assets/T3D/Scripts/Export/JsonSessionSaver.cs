using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;
using UnityEngine.Networking;
using Netherlands3D.Sharing;

public class JsonSessionSaver : MonoBehaviour, IDataSaver
{
    private JSONObject rootObject = new JSONObject();
    const string uploadURL = "https://t3dapi.azurewebsites.net/api/upload/";

    public static JsonSessionSaver Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SaveFloat(string key, float value)
    {
        rootObject[key] = value;
    }

    public void SaveInt(string key, int value)
    {
        rootObject[key] = value;
    }

    public void SaveString(string key, string value)
    {
        rootObject[key] = value;
    }

    public void ExportSaveData(string sessionId)
    {
        Debug.Log("Saving data: " + rootObject.ToString());
        PlayerPrefs.SetString(sessionId, rootObject.ToString());

        StartCoroutine(UploadData(sessionId.ToString(), rootObject.ToString()));
        //PlayerPrefs.SetString(rootObject.ToString());
    }

    private IEnumerator UploadData(string name, string data)
    {
        var uwr = UnityWebRequest.Put(uploadURL + name, data);
        print(uploadURL + name);

        using (uwr)
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                print("saving succeeded");
            }
        }
    }

    public void ClearAllData(string sessionId)
    {
        rootObject = new JSONObject();
        PlayerPrefs.DeleteKey(sessionId);
    }
}
