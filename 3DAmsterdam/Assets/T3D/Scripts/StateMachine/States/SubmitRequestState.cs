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
        if (!ServiceLocator.GetService<T3DInit>().HTMLData.HasSubmitted)
            StartCoroutine(SaveDataWhenCurrentSaveCompletes());
    }

    private IEnumerator SaveDataWhenCurrentSaveCompletes()
    {
        yield return new WaitUntil(() => !SessionSaver.Saver.SaveInProgress); //wait until potential existing save finishes

        ServiceLocator.GetService<T3DInit>().HTMLData.HasSubmitted = true;

        CultureInfo culture = new CultureInfo("nl-NL", false);
        var formattedDate = DateTime.Now.ToString("dd MMMM yyyy", culture);
        ServiceLocator.GetService<T3DInit>().HTMLData.Date = formattedDate;

        SessionSaver.UploadFileToEndpoint(); // export new save data
        SessionSaver.Saver.UploadToEndpointCompleted += Saver_UploadToEndpointCompleted;
    }

    private void Saver_UploadToEndpointCompleted(bool saveSucceeded)
    {
        SessionSaver.Saver.SavingCompleted -= Saver_UploadToEndpointCompleted;
        if (saveSucceeded)
        {
            ShowSuccessMessage(true);
            EndState();
        }
    }

    private void ShowSuccessMessage(bool show)
    {
        inProgressPanel.SetActive(!show);
        successPanel.SetActive(show);
        successText.text = string.Format(defaultSucessString, "<color=\"blue\">" + SubmitPermitRequestState.UserMail + "</color>");
    }
}
