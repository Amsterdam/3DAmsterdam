using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveY : MonoBehaviour {

    //public GameObject button;
    //private Image buttonImage;

    //public void UpDown()
    //{
    //    ManageStates.Instance.selectionState = "Y";
    //}

    //private void Start()
    //{
    //    buttonImage = button.GetComponent<Image>();
    //}

    //private void Update()
    //{
    //    if (ManageStates.Instance.selectionState == "Y")
    //    {
    //        buttonImage.color = Color.yellow;
    //    }
    //    else
    //    {
    //        buttonImage.color = Color.white;
    //    }
    //}

    //private Vector3 mousePosition;
    //private Vector3 currentMousePosition;
    //private float distance;

    //private void OnMouseDown()
    //{
    //    ManageStates.Instance.selectionState = "Mesh";


    //    currentMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
    //                                                  -Camera.main.transform.position.z));

    //    distance = currentMousePosition.y - transform.position.y;
    //}

    //private void OnMouseDrag()
    //{
    //    mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
    //                                           -Camera.main.transform.position.z));

    //    Vector3 distanceAsVector = new Vector3(0, distance, 0);
    //    Vector3 mousePositionAsVector = new Vector3(transform.position.x, mousePosition.y, transform.position.z);

    //    if (ManageStates.Instance.selectionState == "Mesh")
    //    {
    //        transform.parent.parent.parent.position = mousePositionAsVector - distanceAsVector;
    //    }
    //}
}
