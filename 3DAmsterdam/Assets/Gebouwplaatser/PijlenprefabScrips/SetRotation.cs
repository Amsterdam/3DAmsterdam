using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRotation : MonoBehaviour
{
    void Start()
    {
        transform.rotation = transform.parent.rotation;
    }
}