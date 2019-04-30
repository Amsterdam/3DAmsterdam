using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveZ : MonoBehaviour {

    private float mousePosition;
    private Vector3 currentMousePosition;
    private Vector3 direction;

    private float offSet;
    private float distanceToCamera;
    private float moveSpeed = 2f;
    private float angle;

    private void OnMouseDown()
    {
        ManageStates.Instance.selectionState = "Z";

        currentMousePosition = Input.mousePosition;

        offSet = currentMousePosition.y;
    }
    
    private void OnMouseDrag()
    {
        mousePosition = Input.mousePosition.y - offSet;

        if (ManageStates.Instance.selectionState == "Z")
        {
            transform.parent.parent.position += (chooseDirection() * mousePosition * moveSpeed * Time.deltaTime);
        }
    }

    private Vector3 chooseDirection()
    {
        angle = transform.parent.parent.rotation.eulerAngles.y;

        if (angle > 180f && angle < 360f) angle -= 360f;

        if (angle <= 45f && angle >= -90f) direction = Vector3.forward;
        if (angle > 45f && angle < 90f) direction = Vector3.forward;
        if (angle >= 90f && angle <= 180f) direction = Vector3.back;
        if (angle < -90f && angle >= -180f) direction = Vector3.back;


        return direction;
    }
}
