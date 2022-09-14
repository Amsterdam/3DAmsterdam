using System;
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
    private BoundaryFeatureLabelInfo boundaryFeatureLabel;
    [SerializeField]
    private AnnotationLabelInfo annotationLabel;
    [SerializeField]
    private RectTransform boundaryFeaturePanel, annotationPanel;

    [SerializeField]
    private ScrollRect scrollRect;
    private float defaultScrollElasticity;
    
    [SerializeField]
    private  List<GameObject> HideObjectsForAnnotationOnly;

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
        HideObjectsForAnnotationOnly.ForEach(x => {
                x.SetActive(ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel);
            });

        ClearDisplayTags();

        DisplayMetadata();
        DisplayBoundaryFeatures();
        DisplayAnnotations();

        ServiceLocator.GetService<JsonSessionSaver>().EnableAutoSave(false);
    }

    private void ClearDisplayTags()
    {
        foreach (Transform tag in boundaryFeaturePanel)
        {
            Destroy(tag.gameObject);
        }
        foreach (Transform tag in annotationPanel)
        {
            Destroy(tag.gameObject);
        }
    }

    private void DisplayMetadata()
    {
        dateText.text = string.Format(defaultDateText, ServiceLocator.GetService<T3DInit>().HTMLData.Date);
        projectIDText.text = string.Format(defaultProjectIDText, ServiceLocator.GetService<T3DInit>().HTMLData.SessionId.Substring(0, 8));
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
            var newLabel = Instantiate(boundaryFeatureLabel, boundaryFeaturePanel);
            newLabel.SetInfo(bf);
        }

        //disable text if panel is empty
        boundaryFeaturePanel.parent.gameObject.SetActive(boundaryFeaturePanel.childCount > 0);       
    }

    public void DisplayAnnotations()
    {
        foreach (var ann in AnnotationState.AnnotationUIs)
        {
            var newLabel = Instantiate(annotationLabel, annotationPanel);
            newLabel.SetInfo(ann);
        }
        //annotationPanel.parent.GetChild(0).gameObject.SetActive(annotationPanel.childCount > 0);
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

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Q))
        {
            print("setting submitted to false");
            ServiceLocator.GetService<T3DInit>().HTMLData.HasSubmitted = false;
            GoToPreviousState();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            print("uploading to endpoint");
            SessionSaver.UploadFileToEndpoint();
            SessionSaver.Saver.UploadToEndpointCompleted += Saver_UploadToEndpointCompleted;
        }
#endif
    }

    private void Saver_UploadToEndpointCompleted(bool saveSucceeded)
    {
        if (saveSucceeded)
            print("upload success");
        else
            print("upload failed");
    }
}
