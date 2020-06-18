using Amsterdam3D.CameraMotion;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    private CameraControls cameraControls;
    private RectTransform rectTransform;
    private Vector3 lastPointerPosition = Vector3.zero;

    void Start()
    {
        cameraControls = FindObjectOfType<CameraControls>();
        rectTransform = GetComponent<RectTransform>();

        CameraControls.focusPointChanged += MovePointerToPosition;
    }
        
    public void MovePointerToPosition(Vector3 newPosition){
        lastPointerPosition = newPosition;
    }

    void Update()
    {
        var viewportPosition = Camera.main.WorldToViewportPoint(lastPointerPosition);
        rectTransform.anchorMin = viewportPosition;
        rectTransform.anchorMax = viewportPosition;
    }
}
