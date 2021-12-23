using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using System;

public class JSONSessionLoader : MonoBehaviour, IDataLoader
{
    //private bool getUserFeedback = T3DInit.;

    private JSONNode rootObject;// = new JSONObject();
    const string downloadURL = "https://t3dapi.azurewebsites.net/api/download/";
    const string feedbackURL = "https://t3dapi.azurewebsites.net/api/getuserfeedback/";
    private bool hasLoaded = false;

    public static JSONSessionLoader Instance;

    public event IDataLoader.DataLoadedEventHandler LoadingCompleted;
    public bool HasLoaded => hasLoaded;

    private void Awake()
    {
        Instance = this;
    }

    public float LoadFloat(string key)
    {
        var val = rootObject[key];
        if (val)
            return val;
        return default;
    }

    public int LoadInt(string key)
    {
        var val = rootObject[key];
        if (val)
            return val;
        return default;
    }

    public string LoadString(string key)
    {
        var val = rootObject[key];
        if (val)
            return val;
        return default;
    }

    public void ReadSaveData(string sessionId)
    {
        Debug.Log("loading data for session: " + sessionId);

        StartCoroutine(DownloadData(sessionId, ResponseCallback));
    }

    private IEnumerator DownloadData(string name, Action<string> callback = null)
    {
        string url = T3DInit.Instance.IsUserFeedback ? feedbackURL : downloadURL;
        var uwr = UnityWebRequest.Get(url + name);
        print(url + name);

        using (uwr)
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                print("loading succeeded: " + uwr.downloadHandler.text);
                callback?.Invoke(uwr.downloadHandler.text);
            }
        }
    }

    // Callback to act on our response data
    private void ResponseCallback(string data)
    {
        //var jsonString = PlayerPrefs.GetString(sessionId);
        rootObject = JSONNode.Parse(data);

        if (rootObject == null)
        {
            Debug.LogError("parsing session data unsuccesful. Data: " + data);
            rootObject = new JSONObject();
        }

        hasLoaded = rootObject != null;
        LoadingCompleted?.Invoke(hasLoaded);
    }
}
