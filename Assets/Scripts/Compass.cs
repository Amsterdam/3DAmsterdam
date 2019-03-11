using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    public Transform target;
    private Vector3 direction;

    void Update()
    {
        direction.z = target.eulerAngles.y;
        transform.localEulerAngles = direction;
    }
}
