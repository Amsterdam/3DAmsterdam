using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using UnityEngine;
using UnityEngine.UI;

public class UitbouwTransformGizmo : WorldPointFollower
{
    [SerializeField]
    private UitbouwTransformButton moveButton;
    [SerializeField]
    private UitbouwTransformButton rotateButton;

    public bool WantsToMove => moveButton.IsDragging;
    public bool WantsToRotate => rotateButton.IsDragging;
}
