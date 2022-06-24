using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using UnityEngine;
using UnityEngine.UI;

public class AnnotationMarker : MeasurePoint
{
    [SerializeField]
    private WorldPointFollower idMarkerPrefab;
    private WorldPointFollower idLabel;

    public int Id { get; private set; }

    public void Initialize(int id)
    {
        Id = id;

        idLabel = ServiceLocator.GetService<CoordinateNumbers>().CreateGenericWorldPointFollower(idMarkerPrefab);
        idLabel.GetComponentInChildren<Text>().text = id.ToString();
        idLabel.AlignWithWorldPosition(transform.position);
        SetSelectable(true);
    }
}
