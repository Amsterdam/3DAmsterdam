using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RedFamily : Families
{
    Animator anim;

    public override void Start()
    {
        base.Start();

        anim = GetComponentInChildren<Animator>();
        maxSpeed = 3;
    }

    public override void ImageOn()
    {
        if (talking)
        {
            textBox.enabled = true;
            anim.SetBool("isWalking", false);
            anim.SetBool("isTalking", true);
        }
        else
        {
            textBox.enabled = false;
            anim.SetBool("isTalking", false);
            anim.SetBool("isWalking", true);
        }
    }
}
