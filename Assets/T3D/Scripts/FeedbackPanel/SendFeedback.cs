using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SendFeedbackSaveDataContainer : SaveDataContainer
{
    public string UserFeedback;
}

public class SendFeedback : MonoBehaviour
{
    [SerializeField]
    private InputField userFeedbackInputField;

    private SendFeedbackSaveDataContainer saveData;

    const string uploadURL = "api/userfeedback/";
    public Coroutine uploadCoroutine;

    [SerializeField]
    private GameObject successPopup;
    [SerializeField]
    private GameObject failurePopup;
    [SerializeField]
    private GameObject submitFeedbackForm;
    [SerializeField]
    private Button sendButton;
    [SerializeField]
    private Toggle permissionToggle;

    private void InitializeSaveableVariables()
    {
        saveData = new SendFeedbackSaveDataContainer();
    }

    public void SubmitFeedback()
    {
        sendButton.interactable = false;
        permissionToggle.interactable = false;

        InitializeSaveableVariables();

        saveData.UserFeedback = userFeedbackInputField.text;
        //timestamp.SetValue(DateTime.Now.ToString("yyyyMMddHHmmss"));
        string fileName = SessionSaver.SessionId;// + "_" + timestamp.Value;
        ExportSaveData(fileName);

        //RemoveUserMetadata(); //delete the keys again since they will not be needed for session saving
    }

    //private void RemoveUserMetadata()
    //{
    //    SessionSaver.DeleteKey(userFeedbackKey);
    //    //SessionSaver.DeleteKey(timestampKey);
    //}

    public void ExportSaveData(string fileName)
    {
        string saveData = SessionSaver.Saver.SerializeSaveableContainers();
        Debug.Log("Saving data: " + saveData);

        if (uploadCoroutine == null)
        {
            print("making new coroutine");
            uploadCoroutine = StartCoroutine(UploadData(fileName, saveData));
        }
        else
        {
            print("Still waiting for coroutine to return, not saving data");
        }
    }

    private IEnumerator UploadData(string name, string data)
    {
        var uwr = UnityWebRequest.Put(Config.activeConfiguration.T3DAzureFunctionURL + "api/userfeedback/" + name, data);
        uwr.SetRequestHeader("Content-Type", "application/json");

        print(uploadURL + name);
        using (uwr)
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                failurePopup.SetActive(true);
                Debug.LogError(uwr.error);
            }
            else
            {
                print("saving succeeded");
                uploadCoroutine = null;

                sendButton.interactable = true;
                permissionToggle.interactable = true;

                submitFeedbackForm.SetActive(false);
                successPopup.SetActive(true);
            }
        }
    }
}
