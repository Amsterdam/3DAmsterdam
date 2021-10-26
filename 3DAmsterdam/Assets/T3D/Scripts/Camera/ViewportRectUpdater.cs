using UnityEngine;
using UnityEngine.UI;

public class ViewportRectUpdater : MonoBehaviour
{
    public Camera Camera;
    public RectTransform LeftPanel;
    public CanvasScaler CanvasScaler;
    
    private Rect rect;

    private void Start()
    {       
        rect = Camera.rect;
    }

    private void Update()
    {
        //TODO update this only on start and on resize screen
        rect.x = (LeftPanel.rect.width * CanvasScaler.scaleFactor)  / (float)Screen.width;
        Camera.rect = rect;
    }


}