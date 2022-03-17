using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Netherlands3D;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class JsonSessionSaver : MonoBehaviour//, IDataSaver
{
    private JSONNode rootObject = new JSONObject();
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

    public void ExportSaveData(string sessionId)
    {
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

    public string SerializeSaveableContainers()
    {
        rootObject = new JSONObject(); //delete any old data that may be in the rootObject
        foreach (var container in saveDataContainers)
        {
            string jsonContent = JsonUtility.ToJson(container); // Base container's derivative class content variables
            var node = JSONNode.Parse(jsonContent); //todo : not seralize and deserialize here
            rootObject[container.TypeKey].Add(container.InstanceId, node);
        }

        return rootObject.ToString();
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

    public void ClearAllData(string sessionId)
    {
        var persistentTypes = KeepSaveDataOnResetAttribute.GetPersistentTypeKeys();
        List<SaveDataContainer> persistentContainers = new List<SaveDataContainer>();

        foreach (var container in saveDataContainers)
        {
            if (persistentTypes.Contains(container.TypeKey))
            {
                persistentContainers.Add(container);
            }
        }

        saveDataContainers = persistentContainers;
        saveFeedback.SetSaveStatus(SaveFeedback.SaveStatus.WaitingToSave);
        ExportSaveData(sessionId);
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
