using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositioningBlokje : MonoBehaviour
{
    private float startPosY;
    private float startScaleY;

    private void Start()
    {
        startPosY = transform.position.y;
        startScaleY = transform.localScale.y;
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.localScale.y / 2 + startPosY - startScaleY / 2, transform.position.z);
    }
}
