using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    private Vector3 direction = Vector3.zero;
    void Update()
    {
        direction.z = Camera.main.transform.eulerAngles.y;
        transform.localEulerAngles = direction * -1.0f;
    }
}
