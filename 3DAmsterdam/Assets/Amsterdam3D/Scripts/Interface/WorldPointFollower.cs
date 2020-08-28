using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPointFollower : MonoBehaviour
{
    private RectTransform rectTransform;

    [SerializeField]
    private Vector3 worldPosition = Vector3.zero;

    public Vector3 WorldPosition { get => worldPosition; set => worldPosition = value; }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public virtual void AlignWithWorldPosition(Vector3 newWorldPosition)
    {
        WorldPosition = newWorldPosition;
    }

    void Update()
    {
        var viewportPosition = CameraControls.Instance.camera.WorldToViewportPoint(WorldPosition);
        rectTransform.anchorMin = viewportPosition;
        rectTransform.anchorMax = viewportPosition;
    }
}
