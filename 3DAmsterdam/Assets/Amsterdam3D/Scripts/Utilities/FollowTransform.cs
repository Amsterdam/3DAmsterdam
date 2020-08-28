using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField]
    private Transform targetTransform;

    [SerializeField]
    private bool followX = true;
    [SerializeField]
    private bool followY = true;
    [SerializeField]
    private bool followZ = true;
        
    void LateUpdate()
    {
        this.transform.position = new Vector3(
            (followX) ? targetTransform.position.x : transform.position.x,
            (followY) ? targetTransform.position.y : transform.position.y,
            (followZ) ? targetTransform.position.z : transform.position.z
        );
    }
}
