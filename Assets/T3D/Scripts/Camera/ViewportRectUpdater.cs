using Netherlands3D.Cameras;
using UnityEngine;
using UnityEngine.UI;

public class ViewportRectUpdater : MonoBehaviour
{
    //public Camera Camera;
    public RectTransform LeftPanel;
    public RectTransform TopPanel;
    public CanvasScaler CanvasScaler;
    
    private Rect rect;

    private void Start()
    {       
        rect = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.rect;
    }

    private void Update()
    {
        //TODO update this only on start and on resize screen
        rect.x = (LeftPanel.rect.width * CanvasScaler.scaleFactor)  / (float)Screen.width;
        rect.y = -(TopPanel.rect.height * CanvasScaler.scaleFactor)  / (float)Screen.height;
        ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.rect = rect;
    }
}