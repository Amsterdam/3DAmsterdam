using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositioningBlokje : MonoBehaviour
{
    private float startPosY;

    private void Start()
    {
        startPosY = transform.position.y;
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x, startPosY + (transform.localScale.y / 2), transform.position.z);
    }
}
