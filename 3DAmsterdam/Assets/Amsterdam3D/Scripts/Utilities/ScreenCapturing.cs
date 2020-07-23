using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCapturing : MonoBehaviour
{
    [SerializeField]
    private KeyCode screenCaptureKey = KeyCode.S;

    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(screenCaptureKey)){
            ScreenCapture.CaptureScreenshot("Screenshot.png");
        }
    }
}
