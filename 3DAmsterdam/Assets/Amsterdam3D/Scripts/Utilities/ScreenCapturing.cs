using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
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
            var randomName = "Screenshot_" + Random.value + ".png"; 
            ScreenCapture.CaptureScreenshot(randomName);
            print(Application.dataPath + "../" + randomName);
        }
    }
}
