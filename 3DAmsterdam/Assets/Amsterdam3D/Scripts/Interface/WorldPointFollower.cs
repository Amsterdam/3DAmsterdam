using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPointFollower : MonoBehaviour
{
    private RectTransform rectTransform;

    private Vector3 worldPosition = Vector3.zero;
    public Vector3 WorldPosition { get => worldPosition; set => worldPosition = value; }

    public virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public virtual void AlignWithWorldPosition(Vector3 newWorldPosition)
    {
        WorldPosition = newWorldPosition;
    }

    private void Update()
    {
        var viewportPosition = CameraModeChanger.Instance.ActiveCamera.WorldToViewportPoint(worldPosition);
        rectTransform.anchorMin = viewportPosition;
        rectTransform.anchorMax = viewportPosition;
    }
}
