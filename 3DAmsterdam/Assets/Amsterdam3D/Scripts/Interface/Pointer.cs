using Amsterdam3D.CameraMotion;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    private CameraControls cameraControls;
    private RectTransform rectTransform;
    private Vector3 worldPosition = Vector3.zero;

    public Vector3 WorldPosition { get => worldPosition; set => worldPosition = value; }

    void Start()
    {
        cameraControls = FindObjectOfType<CameraControls>();
        rectTransform = GetComponent<RectTransform>();

        CameraControls.focusPointChanged += MovePointerToPosition;
    }
        
    public void MovePointerToPosition(Vector3 newPosition){
        WorldPosition = newPosition;
    }

    void Update()
    {
        var viewportPosition = Camera.main.WorldToViewportPoint(WorldPosition);
        rectTransform.anchorMin = viewportPosition;
        rectTransform.anchorMax = viewportPosition;
    }
}
