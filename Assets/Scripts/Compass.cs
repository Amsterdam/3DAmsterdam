using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    public GameObject manager;
    private int mode;

    public Transform targetGod;
    public Transform targetFPS;
    private Transform target;

    private Vector3 direction;

    void Update()
    {
        mode = manager.GetComponent<ModeManager>().mode;

        if (mode == 1)
        {
            target = targetGod;
        }
        else
        {
            target = targetFPS;
        }

        direction.z = target.eulerAngles.y;
        transform.localEulerAngles = direction;
    }
}
