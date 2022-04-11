using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewSubmittedRequestState : State
{
    [SerializeField]
    private Text /*streetText, zipCodeText,*/ dateText, projectIDText;
    private string defaultDateText, defaultProjectIDText;

    [SerializeField]
    private BoundaryFeatureLabelInfo label;
    [SerializeField]
    private RectTransform labelPanel;

    [SerializeField]
    private Text titleText;

    [SerializeField]
    private ScrollRect scrollRect;
    private float defaultScrollElasticity;

    protected override void Awake()
    {
        base.Awake();      

        defaultDateText = dateText.text;
        defaultProjectIDText = projectIDText.text;

        //scrollRect = GetComponent<ScrollRect>();
        defaultScrollElasticity = scrollRect.elasticity;
    }

    public override void StateEnteredAction()
    {
        titleText.text = "Gemeenteblad";
        DisplayMetadata();
        DisplayBoundaryFeatures();

        JsonSessionSaver.Instance.EnableAutoSave(false);
    }

    private void DisplayMetadata()
    {
        dateText.text = string.Format(defaultDateText, T3DInit.HTMLData.Date);
        projectIDText.text = string.Format(defaultProjectIDText, T3DInit.HTMLData.SessionId.Substring(0, 8));
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

        if (Input.GetKeyDown(KeyCode.Q))
            T3DInit.HTMLData.HasSubmitted = false;
    }
}
