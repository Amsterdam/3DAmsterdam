using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewSubmittedRequestState : State
{
    private SaveableString date;
    private SaveableString projectID;

    [SerializeField]
    private Text dateText, projectIDText;
    private string defaultDateText, defaultProjectIDText;

    protected override void Awake()
    {
        base.Awake();
        date = new SaveableString(HTMLKeys.DATE_KEY);
        projectID = new SaveableString(HTMLKeys.SESSION_ID_KEY);
        defaultDateText = dateText.text;
        defaultProjectIDText = projectIDText.text;
    }

    public override void StateEnteredAction()
    {
        dateText.text = string.Format(defaultDateText, date.Value);
        projectIDText.text = string.Format(defaultProjectIDText, projectID.Value);

        //T3DInit.Instance.IsEditMode = false;
        JsonSessionSaver.Instance.EnableAutoSave(false);
    }
}
