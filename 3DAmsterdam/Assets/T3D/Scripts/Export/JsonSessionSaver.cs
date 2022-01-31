using System.Collections;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class JsonSessionSaver : MonoBehaviour, IDataSaver
{
    const string readOnlyMarker = "$";

    private JSONNode rootObject = new JSONObject();
    const string uploadURL = "https://t3dapi.azurewebsites.net/api/upload/";

    public static JsonSessionSaver Instance;

    public Coroutine uploadCoroutine;
    private bool autoSaveEnabled = true;
    [SerializeField]
    private SaveFeedback saveFeedback;

    public event IDataSaver.DataSavedEventHandler SavingCompleted;

    private void Awake()
    {
        Instance = this;
    }

    public void EnableAutoSave(bool enable)
    {
        autoSaveEnabled = enable && T3DInit.Instance.IsEditMode;
        if (enable)
            StartCoroutine(AutoSaveTimer());
    }

    public void SaveFloat(string key, float value)
    {
        if (rootObject[key] != value)
        {
            rootObject[key] = value;
            saveFeedback.SetSaveStatus(SaveFeedback.SaveStatus.WaitingToSave);
        }
    }

    public void SaveInt(string key, int value)
    {
        if (rootObject[key] != value)
        {
            rootObject[key] = value;
            saveFeedback.SetSaveStatus(SaveFeedback.SaveStatus.WaitingToSave);
        }
    }

    public void SaveString(string key, string value)
    {
        if (rootObject[key] != value)
        {
            rootObject[key] = value;
            saveFeedback.SetSaveStatus(SaveFeedback.SaveStatus.WaitingToSave);
        }
    }

    public void DeleteKey(string key)
    {
        rootObject.Remove(key);
        saveFeedback.SetSaveStatus(SaveFeedback.SaveStatus.WaitingToSave);
    }

    public string GetJsonSaveData()
    {
        return rootObject.ToString();
    }

    public void ExportSaveData(string sessionId)
    {
        //todo: why does rootObject.ToString() not work before building is loaded? Find the issue and remove try/catch
        try
        {
            rootObject.ToString();
        }
        catch
        {
            print("cannot make rootObject string");
            return;
        }

        string saveData = GetJsonSaveData();
        PlayerPrefs.SetString(sessionId, saveData);

        if (uploadCoroutine == null)
        {
            uploadCoroutine = StartCoroutine(UploadData(sessionId, saveData));
        }
        else
        {
            print("Still waiting for coroutine to return, not saving data");
            SavingCompleted?.Invoke(false);
        }
    }

    private IEnumerator UploadData(string name, string data)
    {
        var uwr = UnityWebRequest.Put(uploadURL + name, data);

        using (uwr)
        {
            saveFeedback.SetSaveStatus(SaveFeedback.SaveStatus.Saving);
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(uwr.error);
                SavingCompleted?.Invoke(false);
            }
            else
            {
                saveFeedback.SetSaveStatus(SaveFeedback.SaveStatus.ChangesSaved);
                SavingCompleted?.Invoke(true);
                uploadCoroutine = null;
            }
        }
    }

    public void InitializeRootObject(JSONNode loadedObject)
    {
        rootObject = loadedObject;
        print("new save data: " + rootObject.ToString());
        EnableAutoSave(true);
    }

    public void ClearAllData(string sessionId)
    {
        var newObject = new JSONObject();

        //save readOnly keys
        foreach(var key in rootObject.Keys)
        {
            if (key.Value.StartsWith(readOnlyMarker))
            {
                newObject.Add(key.Value, rootObject[key.Value]);
                print("read only: " + key.Value);
            }
        }

        rootObject = newObject;
        saveFeedback.SetSaveStatus(SaveFeedback.SaveStatus.WaitingToSave);

        ExportSaveData(sessionId);

        PlayerPrefs.DeleteKey(sessionId);
    }

    private IEnumerator AutoSaveTimer()
    {
        while (autoSaveEnabled)
        {
            yield return new WaitForSeconds(5f);
            ExportSaveData(SessionSaver.SessionId);
        }
    }
}
