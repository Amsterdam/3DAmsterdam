using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    private CameraMovement cameraMovement;
    private RectTransform rectTransform;

    private Vector3 lastPointerPosition = Vector3.zero;

    private Canvas parentCanvas;

    void Start()
    {
        cameraMovement = FindObjectOfType<CameraMovement>();
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();

        CameraMovement.focusPointChanged += MovePointerToPosition;
    }
        
    private void MovePointerToPosition(Vector3 newPosition){
        lastPointerPosition = newPosition;
    }

    void Update()
    {
        var viewportPosition = Camera.main.WorldToViewportPoint(lastPointerPosition);
        rectTransform.anchorMin = viewportPosition;
        rectTransform.anchorMax = viewportPosition;
    }
}
