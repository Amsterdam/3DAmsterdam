using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapBehavior : MonoBehaviour
{
    Vector3 eulerRotation;

    void Update()
    {
        // positie (x,z) van de minimap camera wordt gelijk gezet aan de hoofdcamera
        transform.position = new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z);

        // rotatie (y) van de minimap camera wordt gelijk gezet aan de hoofdcamera
        //eulerRotation = new Vector3(transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, transform.eulerAngles.z);
        //transform.rotation = Quaternion.Euler(eulerRotation);
    }
}
