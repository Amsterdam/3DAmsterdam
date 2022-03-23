using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using UnityEngine;
using UnityEngine.UI;

public class UitbouwTransformGizmo : WorldPointFollower
{
    [SerializeField]
    private Toggle moveToggle, rotateToggle;

    public bool MoveModeSelected => moveToggle.isOn;
    public bool RotateModeSelected => rotateToggle.isOn;
}
