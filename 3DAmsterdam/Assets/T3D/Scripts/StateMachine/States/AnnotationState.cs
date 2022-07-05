using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using Netherlands3D.T3D.Uitbouw;
using T3D.Uitbouw;
using UnityEngine;
using UnityEngine.UI;

public class AnnotationState : State
{
    [SerializeField]
    private AnnotationMarker markerPrefab;
    List<AnnotationMarker> annotationMarkers = new List<AnnotationMarker>();

    [SerializeField]
    private RectTransform annotationParent;
    [SerializeField]
    private AnnotationUI annotationPrefab;
    [SerializeField]
    private ScrollToSelected scroll;

    public static List<AnnotationUI> AnnotationUIs = new List<AnnotationUI>();
    public int AmountOfAnnotations => AnnotationUIs.Count;

    public bool AllowSelection { get; set; } = true;

    protected override void Awake()
    {
        base.Awake();
        AnnotationUIs = new List<AnnotationUI>(); //ensure the static list is emptied whenever the scene is reset
    }

    public override void StateLoadedAction()
    {
        if (SessionSaver.LoadPreviousSession)
            LoadSavedAnnotations();
    }

    private void LoadSavedAnnotations()
    {
        var annotationSaveDataNode = SessionSaver.GetJSONNodeOfType(typeof(AnnotationUISaveData).ToString());

        foreach (var node in annotationSaveDataNode)
        {
            //var key = node.Key;
            var data = node.Value;

            //var placedBoundaryFeature = Instantiate(selectedComponent.ComponentObject/*savedPosition, savedRotation*/);
            var parent = data["ParentCityObject"];
            var connectionPoint = data["ConnectionPoint"];
            var text = data["AnnotationText"];
            //placedBoundaryFeature.LoadData(int.Parse(key), prefabName);
            CreateAnnotation(parent, connectionPoint.ReadVector3());
            AnnotationUIs[AmountOfAnnotations - 1].SetText(text);
        }
    }

    //public void RemoveBoundaryFeatureFromSaveData(BoundaryFeature feature)
    //{
    //    //the ids may need to be adjusted to maintain an increment of 1
    //    var lastBf = SavedBoundaryFeatures[SavedBoundaryFeatures.Count - 1];
    //    var deletedID = feature.Id;

    //    //set the id of the last boundary feature to the id of the feature to be deleted to maintain an increment of 1
    //    lastBf.SaveData.SetId(deletedID);
    //    //delete the saveData of the deleted feature.
    //    feature.SaveData.DeleteSaveData();
    //    // delete the bf from the list
    //    SavedBoundaryFeatures.Remove(feature);

    //    return;
    //}

    protected override void Start()
    {
        base.Start();
        RecalculeteContentHeight();
    }

    void Update()
    {
        var maskPlacementPoint = LayerMask.GetMask("ActiveSelection", "Uitbouw");
        var maskMarker = LayerMask.GetMask("SelectionPoints");
        if (AllowSelection && ObjectClickHandler.GetClickOnObject(false, out var hit, maskPlacementPoint, true))
        {
            var parentCityObject = hit.collider.GetComponentInParent<CityObject>();
            CreateAnnotation(parentCityObject.Name, hit.point);
        }
        else if (ObjectClickHandler.GetClickOnObject(false, out hit, maskMarker, false))
        {
            SelectAnnotation(hit.collider.GetComponent<AnnotationMarker>().Id);
        }
    }

    private void SelectAnnotation(int id)
    {
        print("selecting: " + id);
        var selectedAnnotation = AnnotationUIs[id];

        if (!selectedAnnotation.IsOpen)
            selectedAnnotation.ToggleAnnotation();

        RecalculeteContentHeight();
        scroll.SetSelectedChild(id);
    }

    private void CreateAnnotation(string parentCityObject, Vector3 connectionPoint)
    {
        int id = annotationMarkers.Count;
        var annotationmarker = Instantiate(markerPrefab, connectionPoint, Quaternion.identity);
        annotationmarker.SetId(id);
        annotationMarkers.Add(annotationmarker);

        var annotationUI = Instantiate(annotationPrefab, annotationParent);
        AnnotationUIs.Add(annotationUI);
        annotationUI.Initialize(id, parentCityObject, connectionPoint);
        RecalculeteContentHeight();
        scroll.SetSelectedChild(id);
    }

    public void RemoveAnnotation(int id)
    {
        var ui = AnnotationUIs[id];
        AnnotationUIs.Remove(ui);
        Destroy(ui.gameObject);
        var marker = annotationMarkers[id];
        annotationMarkers.Remove(marker);
        Destroy(marker.gameObject);

        RecalculateIds();
        RecalculeteContentHeight();
    }

    private void RecalculateIds()
    {
        for (int i = 0; i < annotationMarkers.Count; i++)
        {
            annotationMarkers[i].SetId(i);
            AnnotationUIs[i].SetId(i);
        }
    }

    public void RecalculeteContentHeight()
    {
        float height = 35f; //top and bottom margin

        foreach (var ann in AnnotationUIs)
        {
            height += ann.GetComponent<RectTransform>().sizeDelta.y;
        }

        annotationParent.sizeDelta = new Vector2(annotationParent.sizeDelta.x, height);
    }
}
