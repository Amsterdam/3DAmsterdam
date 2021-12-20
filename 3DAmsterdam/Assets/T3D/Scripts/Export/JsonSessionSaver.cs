using System.Collections;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class JsonSessionSaver : MonoBehaviour, IDataSaver
{
    private JSONObject rootObject = new JSONObject();
    const string uploadURL = "https://t3dapi.azurewebsites.net/api/upload/";

    public static JsonSessionSaver Instance;

    public Coroutine uploadCoroutine;
    //private Coroutine autoSaveCoroutine;
    private bool autoSaveEnabled = true;
    [SerializeField]
    private SaveFeedback saveFeedback;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        EnableAutoSave(true);
    }

    public void EnableAutoSave(bool enable)
    {
        autoSaveEnabled = enable;
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
        Debug.Log("Saving data: " + saveData);
        PlayerPrefs.SetString(sessionId, saveData);

        if (uploadCoroutine == null)
        {
            print("making new coroutine");
            uploadCoroutine = StartCoroutine(UploadData(sessionId, saveData));
        }
        else
        {
            print("Still waiting for coroutine to return, not saving data");
        }
        //PlayerPrefs.SetString(rootObject.ToString());
    }

    private IEnumerator UploadData(string name, string data)
    {
        var uwr = UnityWebRequest.Put(uploadURL + name, data);
        print(uploadURL + name);
        using (uwr)
        {
            saveFeedback.SetSaveStatus(SaveFeedback.SaveStatus.Saving);
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                saveFeedback.SetSaveStatus(SaveFeedback.SaveStatus.ChangesSaved);
                print("saving succeeded");
                uploadCoroutine = null;
            }
        }
    }

    public void ClearAllData(string sessionId)
    {
        rootObject = new JSONObject();
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
