﻿using Netherlands3D.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneLensFlare : MonoBehaviour
{
    [SerializeField]
    private Transform sun;

    void LateUpdate()
    {
        this.transform.rotation = sun.rotation;
        this.transform.position = CameraModeChanger.Instance.ActiveCamera.transform.position;
    }
}
