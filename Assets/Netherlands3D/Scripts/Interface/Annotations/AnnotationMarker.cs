using System;
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

    [SerializeField]
    private Color selectedColor;
    private Color normalColor;

    void Awake()
    {
        //SetId(id);

        idLabel = ServiceLocator.GetService<CoordinateNumbers>().CreateGenericWorldPointFollower(idMarkerPrefab);
        idLabel.AlignWithWorldPosition(transform.position);
        SetSelectable(true);
        normalColor = idLabel.GetComponent<Image>().color;
    }

    public void SetId(int id)
    {
        Id = id;
        idLabel.GetComponentInChildren<Text>().text = (id + 1).ToString();
    }

    private void OnDestroy()
    {
        if (idLabel)
            Destroy(idLabel.gameObject);
    }

    public void SetSelectedColor(bool selected)
    {
        idLabel.GetComponent<Image>().color = selected ? selectedColor : normalColor;
    }
}
