using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class SubmitRequestState : State
{
    [SerializeField]
    private GameObject inProgressPanel;
    [SerializeField]
    private GameObject successPanel;
    [SerializeField]
    private Text successText;
    private string defaultSucessString;

    protected override void Awake()
    {
        defaultSucessString = successText.text;
    }

    public override void StateEnteredAction()
    {
        ShowSuccessMessage(false);
        StartCoroutine(SaveDataWhenCurrentSaveCompletes());
    }

    private IEnumerator SaveDataWhenCurrentSaveCompletes()
    {
        yield return new WaitUntil(() => !SessionSaver.Saver.SaveInProgress); //wait until potential existing save finishes

        T3DInit.HTMLData.HasSubmitted = true;

        CultureInfo culture = new CultureInfo("nl-NL", false);
        var formattedDate = DateTime.Now.ToString("dd MMMM yyyy", culture);
        T3DInit.HTMLData.Date= formattedDate;

        SessionSaver.ExportSavedData(); // export new save data
        SessionSaver.Saver.SavingCompleted += Saver_SavingCompleted;
    }

    private void Saver_SavingCompleted(bool saveSucceeded)
    {
        if (saveSucceeded)
        {
            ShowSuccessMessage(true);
        }
        SessionSaver.Saver.SavingCompleted -= Saver_SavingCompleted;
    }

    private void ShowSuccessMessage(bool show)
    {
        inProgressPanel.SetActive(!show);
        successPanel.SetActive(show);
        successText.text = string.Format(defaultSucessString, "<color=\"blue\">" + SubmitPermitRequestState.UserMail+"</color>");
    }
}