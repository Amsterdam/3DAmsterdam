using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveXZ : MonoBehaviour
{
    Vector3 distanceFromObject;
    Vector3 offSet;
    Vector3 updatedPos;
    Vector3 updatedDis;

    float startPosY;

    void OnMouseDown()
    {
        ManageStates.Instance.selectionState = "XZ";

        startPosY = transform.parent.parent.position.y;

        distanceFromObject = Camera.main.WorldToScreenPoint(transform.parent.parent.position);
        offSet = Input.mousePosition - distanceFromObject;
    }

    void OnMouseDrag()
    {
        updatedDis = Input.mousePosition - offSet;
        updatedPos = Camera.main.ScreenToWorldPoint(updatedDis);

        if (ManageStates.Instance.selectionState == "XZ")
        {
            transform.parent.parent.position = new Vector3(updatedPos.x, startPosY, updatedPos.z);
        }
    }
}














//    private float xPos;
//    private float yPos;
//    private float zPos;

//    private Vector3 mousePosition;
//    private Vector3 currentMousePositionX;
//    private Vector3 currentMousePositionZ;
//    private float distanceX;
//    private float distanceZ;

//    private void OnMouseDown()
//    {
//        xPos = transform.position.x;
//        yPos = transform.position.y;
//        zPos = transform.position.z;

//        Ray rayX = Camera.main.ScreenPointToRay(Input.mousePosition);
//        RaycastHit hitX;

//        if (Physics.Raycast(rayX, out hitX))
//        {
//            currentMousePositionX = new Vector3(hitX.point.x, yPos, zPos);
//        }

//        Ray rayZ = Camera.main.ScreenPointToRay(Input.mousePosition);
//        RaycastHit hitZ;

//        if (Physics.Raycast(rayZ, out hitZ))
//        {
//            currentMousePositionX = new Vector3(xPos, yPos, hitZ.point.z);
//        }

//        distanceX = currentMousePositionX.x - transform.position.x;
//        distanceZ = currentMousePositionZ.z - transform.position.z;
//    }

//    private void OnMouseDrag()
//    {
//        mousePosition = getMousePosition(mousePosition);

//        Vector3 distanceAsVectorX = new Vector3(distanceX, 0, 0);
//        Vector3 distanceAsVectorZ = new Vector3(0, 0, distanceZ);
//        Vector3 distanceAsVector = distanceAsVectorX + distanceAsVectorZ;

//        transform.parent.parent.position = mousePosition - distanceAsVector;
//    }


//    private Vector3 getMousePosition(Vector3 position)
//    {
//        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//        RaycastHit hit;

//        if (Physics.Raycast(ray, out hit))
//        {
//            position = new Vector3(hit.point.x, yPos, hit.point.z);
//        }

//        return position;
//    }
//}

//    public float movespeed = 10f;
//    private float Ypos;
//    private float Zpos;

//    public Vector3 locatie;

//    void Start()
//    {

//        Ypos = transform.position.y;
//        Zpos = transform.position.z;

//    }

//    private void OnMouseDown()
//    {
//        Ypos = transform.position.y;
//        Zpos = transform.position.z;
//    }

//    private void OnMouseDrag()
//    {

//        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//        RaycastHit hit;

//        int layermask = 9;

//        if (Physics.Raycast(ray, out hit, 10000, layermask))
//        {
//            locatie = hit.point;

//        }


//        Vector3 mousePosition = new Vector3(locatie.x, Ypos, locatie.z);
//        transform.parent.parent.position = mousePosition;
//    }
//}
