using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using System;
using Netherlands3D;

public class JSONSessionLoader : MonoBehaviour, IDataLoader
{
    private JSONNode rootObject;// = new JSONObject();
    private JSONNode rootObject2;
    const string downloadURL = "api/download/";
    const string feedbackURL = "api/getuserfeedback/";
    private bool hasLoaded = false;

    public static JSONSessionLoader Instance;

    public event IDataLoader.DataLoadedEventHandler LoadingCompleted;
    public bool HasLoaded => hasLoaded;

    [TextArea]
    public string TestString;

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

    public bool LoadBool(string key)
    {
        var val = rootObject[key];
        if (val)
            return val;
        return default;
    }

    public void ReadSaveData(string sessionId)
    {
        //Debug.Log("loading data for session: " + sessionId);

        StartCoroutine(DownloadData(sessionId, ResponseCallback));
    }

    public bool TryGetJson(string type, string instanceId, out string json)
    {
        if (rootObject2 == null)
        {
            Debug.LogError("rootObject2 is null");
        }

        var node = rootObject2[type][instanceId];
        json = node.ToString();

        return node != null;
    }

    private IEnumerator DownloadData(string name, Action<string> callback = null)
    {
        string url = Config.activeConfiguration.T3DAzureFunctionURL;
        url += T3DInit.Instance.IsUserFeedback ? feedbackURL : downloadURL;
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
        rootObject = JSON.Parse(data);
        print("data received from server parsing teststring2: " + TestString);
        rootObject2 = JSON.Parse(TestString);

        if (rootObject == null)
        {
            Debug.LogError("parsing session data unsuccesful. Data: " + data);
            rootObject = new JSONObject();
        }

        if (rootObject2 == null)
        {
            Debug.LogError("parsing new data unsuccesful. Data: " + TestString);
            rootObject = new JSONObject();
        }

        hasLoaded = rootObject != null;

        if (hasLoaded)
        {
            JsonSessionSaver.Instance.InitializeRootObject(rootObject, rootObject2); //if there are default values present in the loaded data, put them in the save data to avoid deleting them when they remain unused
        }

        LoadingCompleted?.Invoke(hasLoaded);
    }
}
