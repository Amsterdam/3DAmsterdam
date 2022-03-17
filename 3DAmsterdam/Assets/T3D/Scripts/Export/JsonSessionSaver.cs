using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KeyValueStore;
using Netherlands3D;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class JsonSessionSaver : MonoBehaviour, IDataSaver
{
    const string readOnlyMarker = "$";

    private JSONNode rootObject = new JSONObject();
    private JSONNode rootObject2 = new JSONObject();
    const string uploadURL = "api/upload/";

    public static JsonSessionSaver Instance;

    public Coroutine uploadCoroutine;
    private bool autoSaveEnabled = true;
    [SerializeField]
    private SaveFeedback saveFeedback;

    public event IDataSaver.DataSavedEventHandler SavingCompleted;
    public bool SaveInProgress => uploadCoroutine != null;

    private List<SaveDataContainer> saveDataContainers = new List<SaveDataContainer>();

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

        rootObject[key] = new Vector3();
    }

    public void SaveBool(string key, bool value)
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

        //string saveData = GetJsonSaveData();
        string saveData = SerializeSaveableContainers();
        print(saveData);

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

    public void AddContainer(SaveDataContainer saveDataContainer)
    {
        saveDataContainers.Add(saveDataContainer);
    }

    public void RemoveContainer(SaveDataContainer saveDataContainer)
    {
        saveDataContainers.Remove(saveDataContainer);
    }

    private string SerializeSaveableContainers()
    {
        rootObject2 = new JSONObject(); //delete any old data that may be in the rootObject
        foreach (var container in saveDataContainers)
        {
            string jsonContent = JsonUtility.ToJson(container); // Base container's derivative class content variables
            var node = JSONNode.Parse(jsonContent); //todo : not seralize and deserialize here
            rootObject2[container.TypeKey].Add(container.InstanceId, node);
        }

        return rootObject2.ToString();
    }

    private IEnumerator UploadData(string name, string data)
    {
        var url = Config.activeConfiguration.T3DAzureFunctionURL + uploadURL + name;
        var uwr = UnityWebRequest.Put(url, data);

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
            }
            uploadCoroutine = null;
        }
    }

    public void InitializeRootObject(JSONNode loadedObject, JSONNode newObject)
    {
        rootObject = loadedObject;
        rootObject2 = newObject;
        print("new save data: " + rootObject.ToString());
        print("new save data2: " + rootObject2.ToString());
        EnableAutoSave(true);
    }

    public void ClearAllData(string sessionId)
    {
        var newObject = new JSONObject();

        //save readOnly keys
        foreach (var key in rootObject.Keys)
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
