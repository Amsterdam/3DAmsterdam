using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConvertCoordinates;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class DebugSettings : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;

    public static Vector3RD PositionRD = new Vector3RD(138350.607, 455582.274, 0);//Stadhouderslaan 79 Utrecht
    public static string BagId = "0344100000021804";//Stadhouderslaan 79 Utrecht

    public static bool UploadedModel = false;
    public static bool IsEditMode = true;
    public static bool IsUserFeedback = false;
    public static string BimModelId = "61a57eba0a6448f8eaacf9e9";
    public static string BimModelVersionId = "1";

    [SerializeField]
    private Toggle uploadedModelToggle, isEditModeToggle, isUserFeedbackToggle;
    [SerializeField]
    private InputField bimModelIdInputField, bimModelVersionIdInputField;

    private void Awake()
    {
        uploadedModelToggle.SetIsOnWithoutNotify(UploadedModel);
        isEditModeToggle.SetIsOnWithoutNotify(IsEditMode);
        isUserFeedbackToggle.SetIsOnWithoutNotify(IsUserFeedback);

        bimModelIdInputField.SetTextWithoutNotify(BimModelId);
        bimModelVersionIdInputField.SetTextWithoutNotify(BimModelVersionId);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TogglePanel();
        }
    }

    void TogglePanel()
    {
        panel.SetActive(!panel.activeInHierarchy);
    }
#endif

    //public void SetAddressToErrorBuilding()
    //{
    //    PositionRD = new Vector3RD(136932.03, 454272.937, 0); // measurement error building: 3523AA, 10
    //    BagId = "0344100000035416";// measurement error building : 3523AA, 10
    //}

    public void SetUploadedModel(bool isUploaded)
    {
        UploadedModel = isUploaded;
    }

    public void SetEditMode(bool isEditMode)
    {
        IsEditMode = isEditMode;
    }

    public void SetIsUserFeedback(bool isUserFeedback)
    {
        IsUserFeedback = isUserFeedback;
    }

    public void SetBimModelID(string id)
    {
        BimModelId = id;
    }

    public void SetBimModelVersionId(string id)
    {
        BimModelVersionId = id;
    }

    public void Reload()
    {
        SessionSaver.LoadPreviousSession = false;
        SessionSaver.ClearAllSaveData();
        RestartScene();
    }

    private void RestartScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
