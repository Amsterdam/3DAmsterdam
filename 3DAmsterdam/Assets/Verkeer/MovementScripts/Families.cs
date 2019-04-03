using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Animations;

public class Families : NavPathFollow
{
    public float maxSpeed;

    public bool talking = false;
    public Image textBox;
    public Rigidbody rb;

    public virtual void Start()
    {
        textBox.enabled = false;
        rb = GetComponent<Rigidbody>();
    }

    public virtual void Update()
    {
        ImageOn();

        if (talking != true)
        {
            FollowRoute();
        }
    }


    public virtual void ImageOn()
    {
        if (talking)
        {
            textBox.enabled = true;
        }
        else
        {
            textBox.enabled = false;
        }
    }
}