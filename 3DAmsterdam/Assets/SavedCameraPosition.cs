using Netherlands3D.ObjectInteraction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SavedCameraPosition : Interactable
{
    void Start()
    {
        this.transform.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);
    }

    public override void Select()
    {
        base.Select();
        Camera.main.transform.SetPositionAndRotation(transform.position, transform.rotation);
    }
}
