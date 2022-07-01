using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.UI;

public class Annotator : MonoBehaviour
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

    List<AnnotationUI> annotationUIs = new List<AnnotationUI>();

    public bool AllowSelection { get; set; } = true;

    private void Start()
    {
        RecalculeteContentHeight();
    }

    void Update()
    {
        var maskPlacementPoint = LayerMask.GetMask("ActiveSelection", "Uitbouw");
        var maskMarker = LayerMask.GetMask("SelectionPoints");
        if (AllowSelection && ObjectClickHandler.GetClickOnObject(false, out var hit, maskPlacementPoint, true))
        {
            CreateAnnotation(hit.point);
        }
        else if (ObjectClickHandler.GetClickOnObject(false, out hit, maskMarker, false))
        {
            SelectAnnotation(hit.collider.GetComponent<AnnotationMarker>().Id);
        }
    }

    private void SelectAnnotation(int id)
    {
        print("selecting: " + id);
        var selectedAnnotation = annotationUIs[id];

        if (!selectedAnnotation.IsOpen)
            selectedAnnotation.ToggleAnnotation();

        RecalculeteContentHeight();
        scroll.SetSelectedChild(id);
    }

    private void CreateAnnotation(Vector3 connectionPoint)
    {
        int id = annotationMarkers.Count;
        var annotationmarker = Instantiate(markerPrefab, connectionPoint, Quaternion.identity);
        annotationmarker.SetId(id);
        annotationMarkers.Add(annotationmarker);

        var annotationUI = Instantiate(annotationPrefab, annotationParent);
        annotationUIs.Add(annotationUI);
        annotationUI.SetId(id);
        RecalculeteContentHeight();
        scroll.SetSelectedChild(id);
    }

    public void RemoveAnnotation(int id)
    {
        var ui = annotationUIs[id];
        annotationUIs.Remove(ui);
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
            annotationUIs[i].SetId(i);
        }
    }

    public void RecalculeteContentHeight()
    {
        float height = 35f; //top and bottom margin

        foreach (var ann in annotationUIs)
        {
            height += ann.GetComponent<RectTransform>().sizeDelta.y;
        }

        annotationParent.sizeDelta = new Vector2(annotationParent.sizeDelta.x, height);
    }
}
