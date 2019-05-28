using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RotateY : MonoBehaviour {

    private float rotSpeed = 15;

    void OnMouseDrag()
    {
        float rotY = Input.GetAxis("Mouse X") * rotSpeed + Input.GetAxis("Mouse Y") * -rotSpeed;

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            transform.parent.parent.RotateAround(transform.parent.parent.GetComponent<Collider>().bounds.center, Vector3.up, rotY);
        }
    }
}
