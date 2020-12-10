using Amsterdam3D.CameraMotion;
using Amsterdam3D.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transformable : MonoBehaviour
{
    // Update is called once per frame
    void OnMouseUp()
    {
        //ActivateGizmo


        //Activate Transform panel in sidemenu
        ObjectProperties.Instance.OpenPanel(this.name);
        ObjectProperties.Instance.RenderThumbnailFromPosition(CameraModeChanger.Instance.ActiveCamera.transform.position,this.transform.position);
        ObjectProperties.Instance.AddTransformPanel(gameObject);
    }
}
