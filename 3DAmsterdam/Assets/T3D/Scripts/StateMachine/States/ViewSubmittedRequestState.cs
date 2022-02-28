using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewSubmittedRequestState : State
{
    private SaveableString street;
    private SaveableString number;
    private SaveableString zipCode;
    private SaveableString city;

    private SaveableString date;
    private SaveableString projectID;

    [SerializeField]
    private Text streetText, zipCodeText, dateText, projectIDText;
    private string defaultStreetText, defaultZipCodeText, defaultDateText, defaultProjectIDText;

    protected override void Awake()
    {
        base.Awake();
        street = new SaveableString(HTMLKeys.STREET_KEY);
        number = new SaveableString(HTMLKeys.HOUSE_NUMBER_KEY);
        zipCode = new SaveableString(HTMLKeys.ZIP_CODE_KEY);
        city = new SaveableString(HTMLKeys.CITY_KEY);
        date = new SaveableString(HTMLKeys.DATE_KEY);
        projectID = new SaveableString(HTMLKeys.SESSION_ID_KEY);

        defaultStreetText = streetText.text;
        defaultZipCodeText = zipCodeText.text;
        defaultDateText = dateText.text;
        defaultProjectIDText = projectIDText.text;
    }

    public override void StateEnteredAction()
    {
        streetText.text = string.Format(defaultStreetText, street.Value, number.Value);
        zipCodeText.text = string.Format(defaultZipCodeText, zipCode.Value, city.Value);
        dateText.text = string.Format(defaultDateText, date.Value);
        projectIDText.text = string.Format(defaultProjectIDText, projectID.Value.Substring(0, 8));

        //T3DInit.Instance.IsEditMode = false;
        JsonSessionSaver.Instance.EnableAutoSave(false);
    }

    protected override void LoadSavedState()
    {
        //base.LoadSavedState(); //don't go to any next state in this state
    }
}
