using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SendFeedback : MonoBehaviour
{
    //[SerializeField]
    //private InputField hardwareInfoInputField;
    [SerializeField]
    private InputField userFeedbackInputField;

    private string userFeedbackKey;
    private SaveableString userFeedback;

    private string timestampKey;
    private SaveableString timestamp;

    private void Awake()
    {
        userFeedbackKey = GetType() + ".userFeedback";
        userFeedback = new SaveableString(userFeedbackKey);

        timestampKey = GetType() + ".userFeedback";
        timestamp = new SaveableString(timestampKey);
    }

    public void SubmitFeedback()
    {
        userFeedback.SetValue(userFeedbackInputField.text);
        timestamp.SetValue(DateTime.Now.ToString());
        SessionSaver.ExportSavedData();
    }
}
