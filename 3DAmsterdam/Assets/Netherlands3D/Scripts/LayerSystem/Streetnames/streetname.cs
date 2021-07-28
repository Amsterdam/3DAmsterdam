using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Cameras;

public class streetname : MonoBehaviour
{
    float currentAngle;
    // Start is called before the first frame update
    void Start()
    {

        currentAngle = transform.transform.localRotation.eulerAngles.y;
        if (currentAngle <0)
        {
            currentAngle += 360;
        }

    }

    // Update is called once per frame
    void Update()
    {
       float cameraAngle = CameraModeChanger.Instance.ActiveCamera.transform.rotation.eulerAngles.y;
        bool flip = false;
        float angleDelta;
        angleDelta = Mathf.DeltaAngle(cameraAngle, currentAngle);
        if (Mathf.Abs(angleDelta)>90)
        {
            flip = true;
        }

        if (flip)
        {
            transform.Rotate(Vector3.forward, 180f, Space.Self);
            currentAngle = transform.transform.localRotation.eulerAngles.y;
                if (currentAngle<0)
                {
                    currentAngle += 360;
                }
        
        }

    }
}
