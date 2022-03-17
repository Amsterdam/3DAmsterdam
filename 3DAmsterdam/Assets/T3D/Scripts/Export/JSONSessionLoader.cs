using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using System;
using Netherlands3D;

public class JSONSessionLoader : MonoBehaviour//, IDataLoader
{
    private JSONNode rootObject;// = new JSONObject();
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

    public void ReadSaveData(string sessionId)
    {
        //Debug.Log("loading data for session: " + sessionId);

        StartCoroutine(DownloadData(sessionId, ResponseCallback));
    }

    public JSONNode GetJSONNodeOfType(string typeKey)
    {
        if (rootObject == null)
        {
            Debug.LogError("rootObject is null");
        }
        return rootObject[typeKey];
    }

    public bool TryGetJson(string type, string instanceId, out string json)
    {
        if (rootObject == null)
        {
            Debug.LogError("rootObject is null");
        }

        var node = rootObject[type][instanceId];
        json = node.ToString();

        return node != null;
    }

    private IEnumerator DownloadData(string name, Action<string> callback = null)
    {
        string url = Config.activeConfiguration.T3DAzureFunctionURL;
        url += T3DInit.HTMLData.IsUserFeedback ? feedbackURL : downloadURL;
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
                //todo: replace with text from call
                print("loading succeeded: " + uwr.downloadHandler.text);
                callback?.Invoke(uwr.downloadHandler.text);

                //print("using TestString: " + TestString);
                //callback?.Invoke(TestString);
            }
        }
    }

    // Callback to act on our response data
    private void ResponseCallback(string data)
    {
        //var jsonString = PlayerPrefs.GetString(sessionId);
        rootObject = JSON.Parse(data);

        if (rootObject == null)
        {
            Debug.LogError("parsing session data unsuccesful. Data: " + data);
            rootObject = new JSONObject();
        }

        hasLoaded = rootObject != null;

        LoadingCompleted?.Invoke(hasLoaded);

        if (hasLoaded)
        {
            JsonSessionSaver.Instance.EnableAutoSave(true); //if there are default values present in the loaded data, put them in the save data to avoid deleting them when they remain unused
        }
    }
}
