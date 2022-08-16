using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using System;
using Netherlands3D;

public class JSONSessionLoader : MonoBehaviour, IUniqueService//, IDataLoader
{
    private JSONNode rootObject;// = new JSONObject();
    const string downloadURL = "api/download/";
    const string feedbackURL = "api/getuserfeedback/";
    private bool hasLoaded = false;

    public event IDataLoader.DataLoadedEventHandler LoadingCompleted;
    public bool HasLoaded => hasLoaded;

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
        url += ServiceLocator.GetService<T3DInit>().HTMLData.IsUserFeedback ? feedbackURL : downloadURL;
        var uwr = UnityWebRequest.Get(url + name);
        print(url + name);

        using (uwr)
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                ErrorService.GoToErrorPage("Could not load session data. " + uwr.error);
            }
            else
            {
                //todo: replace with text from call
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

        if (rootObject == null)
        {
            ErrorService.GoToErrorPage("parsing session data unsuccesful. Data: " + data);
            rootObject = new JSONObject();
        }

        hasLoaded = rootObject != null;

        LoadingCompleted?.Invoke(hasLoaded);

        if (hasLoaded)
        {
            ServiceLocator.GetService<JsonSessionSaver>().EnableAutoSave(true); //if there are default values present in the loaded data, put them in the save data to avoid deleting them when they remain unused
        }
    }
}
