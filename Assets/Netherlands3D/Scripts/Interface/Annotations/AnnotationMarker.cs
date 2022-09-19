using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using UnityEngine;
using UnityEngine.UI;

public class AnnotationMarker : MeasurePoint
{
    [SerializeField]
    private WorldPointFollower idMarkerPrefab;
    private WorldPointFollower idLabel;

    public int Id { get; private set; }

    private Image idLabelImage;
    private Text idLabelText;

    [SerializeField]
    private Color selectedColor;
    private Color normalColor;

    private Camera cam;

    void Awake()
    {
        //SetId(id);

        idLabel = ServiceLocator.GetService<CoordinateNumbers>().CreateGenericWorldPointFollower(idMarkerPrefab);
        idLabel.AlignWithWorldPosition(transform.position);
        idLabelImage = idLabel.GetComponent<Image>();
        idLabelText = idLabel.GetComponentInChildren<Text>();
        SetSelectable(true);
        normalColor = idLabel.GetComponent<Image>().color;
    }

    public void SetId(int id)
    {
        Id = id;
        idLabelText.text = (id + 1).ToString();
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

    protected override void Update()
    {
        base.Update();
        cam = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera;
        var dir = cam.transform.position - idLabel.WorldPosition;
        var imageColor = idLabelImage.color;
        var textColor = idLabelText.color;
        if (Physics.Raycast(idLabel.WorldPosition, dir.normalized, out var hit, dir.magnitude))
        {
            imageColor.a = 0.3f;
            textColor.a = 0.3f;
            print(hit.collider.gameObject.name);
        }
        else
        {
            imageColor.a = 1f;
            textColor.a = 1f;
        }

        idLabelImage.color = imageColor;
        idLabelText.color = textColor;
    }
}
