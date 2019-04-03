using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveX : MonoBehaviour {

    //Vector3 distanceFromObject;
    //Vector3 offSet;
    //Vector3 updatedPos;
    //Vector3 updatedDis;

    //float startPosY;
    //float startPosZ;

    //private void OnMouseDown()
    //{
    //    ManageStates.Instance.selectionState = "X";

    //    startPosY = transform.parent.parent.position.y;
    //    startPosZ = transform.parent.parent.position.z;

    //    distanceFromObject = Camera.main.WorldToScreenPoint(transform.parent.parent.position);
    //    offSet = Input.mousePosition - distanceFromObject;
    //}

    //private void OnMouseDrag()
    //{
    //    updatedDis = Input.mousePosition - offSet;
    //    updatedPos = Camera.main.ScreenToWorldPoint(updatedDis);

    //    if (ManageStates.Instance.selectionState == "X")
    //    {
    //        transform.parent.parent.position = new Vector3(updatedPos.x, startPosY, startPosZ);
    //    }
    //}



    private float mousePosition;
    private Vector3 currentMousePosition;
    private Vector3 direction;

    private float offSet; // de afstand tussen de muis en het object
    private float moveSpeed = 0.15f;
    private float angle;

    private void OnMouseDown()
    {
        ManageStates.Instance.selectionState = "X";

        // de positie die het muis op dat moment heeft wordt uitgerekend
        currentMousePosition = Input.mousePosition;

        offSet = currentMousePosition.x;
    }

    private void OnMouseDrag()
    {
        float directionH = Input.GetAxis("Horizontal");
        float directionV = Input.GetAxis("Vertical");


        // De huidige muispositie wordt uitgerekend
        mousePosition = Input.mousePosition.x - offSet;

        if (ManageStates.Instance.selectionState == "X")
        {
            transform.parent.parent.Translate(chooseDirection() * mousePosition * moveSpeed * Time.deltaTime);
        }
    }

    private Vector3 chooseDirection()
    {
        angle = transform.parent.parent.rotation.eulerAngles.y;

        if (angle > 180f && angle < 360f) angle -= 360f;

        if (angle <= 45f && angle >= -90f) direction = Vector3.right;
        if (angle > 45f && angle < 90f) direction = Vector3.right;
        if (angle >= 90f && angle <= 180f) direction = Vector3.left;
        if (angle < -90f && angle >= -180f) direction = Vector3.left;

        return direction;
    }
}
