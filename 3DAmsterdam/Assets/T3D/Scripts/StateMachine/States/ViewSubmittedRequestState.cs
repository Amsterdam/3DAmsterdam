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
    private Text /*streetText, zipCodeText,*/ dateText, projectIDText;
    private string defaultStreetText, defaultZipCodeText, defaultDateText, defaultProjectIDText;

    [SerializeField]
    private BoundaryFeatureLabelInfo label;
    [SerializeField]
    private RectTransform labelPanel;

    [SerializeField]
    private Text titleText;

    private ScrollRect scrollRect;
    private float defaultScrollElasticity;

    protected override void Awake()
    {
        base.Awake();
        street = new SaveableString(HTMLKeys.STREET_KEY);
        number = new SaveableString(HTMLKeys.HOUSE_NUMBER_KEY);
        zipCode = new SaveableString(HTMLKeys.ZIP_CODE_KEY);
        city = new SaveableString(HTMLKeys.CITY_KEY);
        date = new SaveableString(HTMLKeys.DATE_KEY);
        projectID = new SaveableString(HTMLKeys.SESSION_ID_KEY);

        //defaultStreetText = streetText.text;
        //defaultZipCodeText = zipCodeText.text;
        defaultDateText = dateText.text;
        defaultProjectIDText = projectIDText.text;

        scrollRect = GetComponent<ScrollRect>();
        defaultScrollElasticity = scrollRect.elasticity;
    }

    public override void StateEnteredAction()
    {
        titleText.text = "Gemeenteblad";
        DisplayMetadata();
        DisplayBoundaryFeatures();

        //T3DInit.Instance.IsEditMode = false;
        JsonSessionSaver.Instance.EnableAutoSave(false);
    }

    private void DisplayMetadata()
    {
        //streetText.text = string.Format(defaultStreetText, street.Value, number.Value);
        //zipCodeText.text = string.Format(defaultZipCodeText, zipCode.Value, city.Value);
        dateText.text = string.Format(defaultDateText, date.Value);
        projectIDText.text = string.Format(defaultProjectIDText, projectID.Value.Substring(0, 8));
    }

    protected override void LoadSavedState()
    {
        //don't call the base function: don't go to any next state in this state
        //base.LoadSavedState(); 
    }

    public void DisplayBoundaryFeatures()
    {
        foreach (var bf in PlaceBoundaryFeaturesState.SavedBoundaryFeatures)
        {
            var newLabel = Instantiate(label, labelPanel);
            newLabel.SetInfo(bf);
        }
    }

    private void Update()
    {
        if (scrollRect.verticalScrollbar.gameObject.activeInHierarchy)
        {
            scrollRect.elasticity = defaultScrollElasticity;
        }
        else
        {
            scrollRect.elasticity = 0;
        }
    }
}
