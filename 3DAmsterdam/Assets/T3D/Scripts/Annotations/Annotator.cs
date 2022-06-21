using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public class Annotator : MonoBehaviour
{
    [SerializeField]
    private WorldPointFollower annotationPrefab;

    public bool AllowSelection { get; set; } = true;

    // Update is called once per frame
    void Update()
    {
        var mask = LayerMask.GetMask("ActiveSelection", "Uitbouw");
        if (AllowSelection && ObjectClickHandler.GetClickOnObject(false, out var hit, mask))
        {
            CreateAnnotation(hit.point);
        }
    }

    private void CreateAnnotation(Vector3 connectionPoint)
    {
        var wpf = ServiceLocator.GetService<CoordinateNumbers>().CreateGenericWorldPointFollower(annotationPrefab);
        wpf.AlignWithWorldPosition(connectionPoint);
        var annotation = wpf.GetComponent<Annotation>();
        annotation.BodyText = "test";
    }
}
