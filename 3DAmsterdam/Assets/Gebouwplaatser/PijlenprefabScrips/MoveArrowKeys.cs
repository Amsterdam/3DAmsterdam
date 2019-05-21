//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MoveArrowKeys : MonoBehaviour {

//    public float moveSpeed;
//    private float rotSpeed = 3;

//    private float directionH;
//    private float directionV;

//    private Color32 originalColor;
    
//	void Start () {
//        originalColor = GameObject.Find("X").GetComponent<Renderer>().material.color;
//    }

//    void Update () {
//        directionH = Input.GetAxis("Horizontal");
//        directionV = Input.GetAxis("Vertical");

//        if (ManageStates.Instance.selectionState == "X" ||
//            ManageStates.Instance.selectionState == "XZ")
//        {
//            // als linker of rechter pijltoetsen ingedrukt worden beweegt object in horizontale richting.
//            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
//            {
//                transform.parent.Translate(directionH * moveSpeed * ManageStates.Instance.genericSpeed, 0, 0);
//            }
//        }

//        if (ManageStates.Instance.selectionState == "Z" ||
//            ManageStates.Instance.selectionState == "XZ")
//        {
//            // als omhoog of omlaag pijltoetsen ingedrukt worden beweegt object in verticale richting.
//            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
//            {
//                transform.parent.Translate(0, 0, directionV * moveSpeed * ManageStates.Instance.genericSpeed);
//            }
//        }

//        // "Mesh" is de Y-state.
//        if (ManageStates.Instance.selectionState == "Y")
//        {
//            // als omhoog of omlaag pijltoetsen ingedrukt worden beweegt object omhoog of omlaag.
//            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
//            {
//                transform.parent.Translate(0, directionV * moveSpeed * ManageStates.Instance.genericSpeed, 0);
//            }
//        }

//        // het object kan geroteerd worden met linker en rechter pijltoetsen.
//        if (ManageStates.Instance.selectionState == "Ry")
//        {
//            if (Input.GetKey(KeyCode.LeftArrow))
//            {
//                //transform.parent.Rotate(0, -rotSpeed * ManageStates.Instance.genericSpeed, 0);
//                transform.parent.RotateAround(transform.parent.GetComponent<Collider>().bounds.center,
//                                              Vector3.up, -rotSpeed * ManageStates.Instance.genericSpeed);
//            }

//            if (Input.GetKey(KeyCode.RightArrow))
//            {
//                //transform.parent.Rotate(0, rotSpeed * ManageStates.Instance.genericSpeed, 0);
//                transform.parent.RotateAround(transform.parent.GetComponent<Collider>().bounds.center,
//                              Vector3.up, rotSpeed * ManageStates.Instance.genericSpeed);
//            }
//        }
//    }
//}
