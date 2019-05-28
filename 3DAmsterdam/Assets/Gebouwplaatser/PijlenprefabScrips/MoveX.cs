using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveX : MonoBehaviour {

    private float mousePosition;
    private Vector3 currentMousePosition;
    private Vector3 direction;
    private Vector3 updatedDis;
    private Vector3 updatedPos;
    private Vector3 distanceFromObject;

    private Vector3 offSet; // de afstand tussen de muis en het object
    private float moveSpeed = 0.5f;
    private float angle;
    private float startPosY;
    private float startPosZ;
    private float angleObject;

    private Vector3 moveVector;


    private void OnMouseDown()
    {
        // de positie die het muis op dat moment heeft wordt uitgerekend
        currentMousePosition = Input.mousePosition;

        startPosY = transform.parent.parent.position.y;
        startPosZ = transform.parent.parent.position.z;

        distanceFromObject = Camera.main.WorldToScreenPoint(transform.parent.parent.position);
        offSet = Input.mousePosition - distanceFromObject;
    }

    private void OnMouseDrag()
    {
        angleObject = transform.parent.parent.eulerAngles.y;
        moveVector = transform.right * Mathf.Log(Camera.main.transform.position.y, 2) * 0.15f;

        if (angleObject > 315f && angleObject < 360f || angleObject >= 0f && angleObject < 45f)
        {
            if (Input.GetAxis("Mouse X") < 0)
            {
                transform.parent.parent.position -= moveVector;
            }
            if (Input.GetAxis("Mouse X") > 0)
            {
                transform.parent.parent.position += moveVector;
            }
        }
        else if (angleObject >= 45f && angleObject < 135f)
        {
            if (Input.GetAxis("Mouse Y") < 0)
            {
                transform.parent.parent.position += moveVector;
            }
            if (Input.GetAxis("Mouse Y") > 0)
            {
                transform.parent.parent.position -= moveVector;
            }
        }
        else if (angleObject >= 135f && angleObject < 225f)
        {
            if (Input.GetAxis("Mouse X") < 0)
            {
                transform.parent.parent.position += moveVector;
            }
            if (Input.GetAxis("Mouse X") > 0)
            {
                transform.parent.parent.position -= moveVector;
            }
        }
        else
        {
            if (Input.GetAxis("Mouse Y") < 0)
            {
                transform.parent.parent.position -= moveVector;
            }
            if (Input.GetAxis("Mouse Y") > 0)
            {
                transform.parent.parent.position += moveVector;
            }
        }

        //updatedDis = Input.mousePosition - offSet;
        //updatedPos = Camera.main.ScreenToWorldPoint(updatedDis);

        //transform.parent.parent.position = new Vector3(updatedPos.x, startPosY, startPosZ);
    }
}
