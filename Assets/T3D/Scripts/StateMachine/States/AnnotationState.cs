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

    public GameObject PreviousButton;

    public static List<AnnotationUI> AnnotationUIs = new List<AnnotationUI>();
    public int AmountOfAnnotations => AnnotationUIs.Count;

    public bool AllowSelection { get; set; } = true;
    public int ActiveSelectedId = 0;

    protected override void Awake()
    {
        base.Awake();
        AnnotationUIs = new List<AnnotationUI>(); //ensure the static list is emptied whenever the scene is reset
    }

    public override int GetDesiredStateIndex()
    {
        //if (ServiceLocator.GetService<T3DInit>().HTMLData == null)
        //    return 0;

        if (ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel == false) return 2;

        return ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall ? 0 : 1;
    }

    public override void StateLoadedAction()
    {
        if (SessionSaver.LoadPreviousSession)
            LoadSavedAnnotations();
    }

    private void LoadSavedAnnotations()
    {
        //PreviousButton.SetActive( ServiceLocator.GetService<T3DInit>().HTMLData.Add3DModel);

        var annotationSaveDataNode = SessionSaver.GetJSONNodeOfType(typeof(AnnotationUISaveData).ToString());

        foreach (var node in annotationSaveDataNode)
        {
            var data = node.Value;

            var parent = data["ParentCityObject"];
            var connectionPoint = data["ConnectionPoint"];
            var text = data["AnnotationText"];
            CreateAnnotation(parent, connectionPoint.ReadVector3());
            AnnotationUIs[AmountOfAnnotations - 1].SetText(text);
        }
    }

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
            CreateAnnotation(parentCityObject.Id, hit.point);
        }
        else if (ObjectClickHandler.GetClickOnObject(false, out hit, maskMarker, false))
        {
            SelectAnnotation(hit.collider.GetComponent<AnnotationMarker>().Id);
        }
    }

    private void SelectAnnotation(int id)
    {
        //print("selecting: " + id);
        AnnotationUIs[ActiveSelectedId].SetSelectedColor(false);
        annotationMarkers[ActiveSelectedId].SetSelectedColor(false);

        var selectedAnnotation = AnnotationUIs[id];
        var selectedMarker= annotationMarkers[id];

        if (!selectedAnnotation.IsOpen)
            selectedAnnotation.ToggleAnnotation();

        selectedAnnotation.SetSelectedColor(true);
        selectedMarker.SetSelectedColor(true);

        RecalculeteContentHeight();
        scroll.SetSelectedChild(id);
        ActiveSelectedId = id;
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

        SelectAnnotation(id);
        //RecalculeteContentHeight();
        //scroll.SetSelectedChild(id);
    }

    public void RemoveAnnotation(int id)
    {
        var ui = AnnotationUIs[id];
        AnnotationUIs.Remove(ui);
        Destroy(ui.gameObject);
        var marker = annotationMarkers[id];
        annotationMarkers.Remove(marker);
        Destroy(marker.gameObject);

        if (id == ActiveSelectedId)
            ActiveSelectedId = 0;

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
        annotationParent.sizeDelta = new Vector2(annotationParent.sizeDelta.x, AnnotationUIs.Count > 0 ? height : 0);        
    }
}
