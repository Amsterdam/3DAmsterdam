using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveZ : MonoBehaviour {

    private float mousePosition;
    private Vector3 currentMousePosition;
    private Vector3 direction;
    private Vector3 updatedDis;
    private Vector3 updatedPos;
    private Vector3 distanceFromObject;

    private Vector3 offSet; // de afstand tussen de muis en het object
    private float moveSpeed = 0.5f;
    private float angle;
    private float startPosX;
    private float startPosY;
    private float startPosZ;

    private float directionH;
    private float directionV;

    public void Update()
    {
        directionH = Input.GetAxis("Horizontal");
        directionV = Input.GetAxis("Vertical");
    }

    private void OnMouseDown()
    {
        ManageStates.Instance.selectionState = "Z";

        // de positie die het muis op dat moment heeft wordt uitgerekend
        currentMousePosition = Input.mousePosition;

        startPosY = transform.parent.parent.position.y;
        startPosZ = transform.parent.parent.position.z;
        startPosX = transform.parent.parent.position.x;

        distanceFromObject = Camera.main.WorldToScreenPoint(transform.parent.parent.position);
        offSet = Input.mousePosition - distanceFromObject;
    }

    private void OnMouseDrag()
    {
        updatedDis = Input.mousePosition - offSet;
        updatedPos = Camera.main.ScreenToWorldPoint(updatedDis);

        if (ManageStates.Instance.selectionState == "Z")
        {
            transform.parent.parent.position = new Vector3(startPosX, startPosY, updatedPos.z);
        }
    }

    //private float mousePosition;
    //private Vector3 currentMousePosition;
    //private Vector3 direction;

    //private float offSet;
    //private float distanceToCamera;
    //private float moveSpeed = 2f;
    //private float angle;

    //private void OnMouseDown()
    //{
    //    ManageStates.Instance.selectionState = "Z";

    //    currentMousePosition = Input.mousePosition;

    //    offSet = currentMousePosition.y;
    //}

    //private void OnMouseDrag()
    //{
    //    mousePosition = Input.mousePosition.y - offSet;

    //    if (ManageStates.Instance.selectionState == "Z")
    //    {
    //        transform.parent.parent.Translate(chooseDirection() * mousePosition * moveSpeed * Time.deltaTime);
    //    }
    //}

    //private Vector3 chooseDirection()
    //{
    //    angle = transform.parent.parent.rotation.eulerAngles.y;

    //    if (angle > 180f && angle < 360f) angle -= 360f;

    //    if (angle <= 45f && angle >= -90f) direction = Vector3.forward;
    //    if (angle > 45f && angle < 90f) direction = Vector3.forward;
    //    if (angle >= 90f && angle <= 180f) direction = Vector3.back;
    //    if (angle < -90f && angle >= -180f) direction = Vector3.back;


    //    return direction;
    //}
}
