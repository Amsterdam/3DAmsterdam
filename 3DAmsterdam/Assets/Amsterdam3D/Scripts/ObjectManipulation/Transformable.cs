using Amsterdam3D.CameraMotion;
using Amsterdam3D.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transformable : MonoBehaviour
{
    // Update is called once per frame
    void OnMouseOver()
    {
        
        //ActivateGizmo
        if(Input.GetMouseButtonDown(1)){
            ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.CUSTOM_OBJECTS);
            ContextPointerMenu.Instance.SetTargetObject(gameObject);
        }
    }
}
