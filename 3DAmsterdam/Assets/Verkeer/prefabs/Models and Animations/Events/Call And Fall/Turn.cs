using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Turn: MonoBehaviour
{
    public void Start()
    {
        this.transform.position = transform.position + new Vector3(0, .1f, 0);
    }

    public void TurnAround()
    {
        transform.rotation = this.transform.rotation * Quaternion.Euler(0, 150f, 0);
        this.transform.position = transform.position + new Vector3(0, .3f, 0);
    }
}
