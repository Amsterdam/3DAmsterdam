using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public class Annotator : MonoBehaviour
{
    [SerializeField]
    private AnnotationMarker markerPrefab;
    List<AnnotationMarker> annotationMarkers = new List<AnnotationMarker>();

    public bool AllowSelection { get; set; } = true;
        
    void Update()
    {
        var maskPlacementPoint = LayerMask.GetMask("ActiveSelection", "Uitbouw");
        var maskMarker = LayerMask.GetMask("SelectionPoints");
        if (AllowSelection && ObjectClickHandler.GetClickOnObject(false, out var hit, maskPlacementPoint, true))
        {
            CreateAnnotation(hit.point);
        }
        else if(ObjectClickHandler.GetClickOnObject(false, out hit, maskMarker, true))
        {
            SelectAnnotation(hit.collider.GetComponent<AnnotationMarker>().Id);
        }
    }

    private void SelectAnnotation(int id)
    {
        print("selecting: " + id);
    }

    private void CreateAnnotation(Vector3 connectionPoint)
    {
        var annotationmarker = Instantiate(markerPrefab, connectionPoint, Quaternion.identity);
        annotationmarker.GetComponent<AnnotationMarker>().Initialize(annotationMarkers.Count);
        annotationMarkers.Add(annotationmarker);
    }
}
