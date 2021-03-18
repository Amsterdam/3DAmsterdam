using UnityEngine;

namespace Netherlands3D.Development
{
    public class ScreenCapturing : MonoBehaviour
    {
        [SerializeField]
        private KeyCode screenCaptureKey = KeyCode.S;
        void Update()
        {
            if (Input.GetKeyDown(screenCaptureKey))
            {
                var randomName = "Screenshot_" + Random.value + ".png";
                ScreenCapture.CaptureScreenshot(randomName);
                print("Saved screenshot to: " + Application.dataPath + "../" + randomName);
            }
        }
    }
}