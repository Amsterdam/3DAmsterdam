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

    private float directionH;
    private float directionV;

    public void Update()
    {
        directionH = Input.GetAxis("Horizontal");
        directionV = Input.GetAxis("Vertical");
    }

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
        updatedDis = Input.mousePosition - offSet;
        updatedPos = Camera.main.ScreenToWorldPoint(updatedDis);

        transform.parent.parent.position = new Vector3(updatedPos.x, startPosY, startPosZ);
    }
}
