using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GreenFamily : Families
{
    public float ignoreRadius;
    public float personRadius;
    public float talkTime;
    public bool talkTimer;
    public float talkingCD;
    public bool talkCooldown;
    Animator anim;

    public override void Start()
    {
        base.Start();

        talkCooldown = false;
        talkTimer = false;

        ignoreRadius = 6f;
        talkingCD = 0;
        talkTime = 0;
        personRadius = 2.5f;

        anim = GetComponentInChildren<Animator>();
    }

    public override void Update()
    {
        base.Update();

        if (talkTimer == false)
        {
            talkTime = 0f;
        }

        Talk();
    }

    //public void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireSphere(agent.transform.position, ignoreRadius);

    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(agent.transform.position, personRadius);
    //}

    public virtual void Talk()
    {
        Families[] others; //Array for containing people

        others = GameObject.FindObjectsOfType<RedFamily>(); //All red family members go in here

        foreach (Families g in others)
        {
            float distance = Vector3.Distance(transform.position, g.transform.position);

            if(personRadius == 0)
            {
                g.talking = false;
                g.agent.speed = 3;
            }

            if (talking)
            {
                if (distance <= ignoreRadius && distance >= personRadius)
                {
                     g.agent.SetDestination(g.positions[g.currentPos].transform.position + Vector3.left * 20f);
                }
            }

            if (distance <= personRadius)
            {
                talking = true;
                g.talking = true;

                if (talking)
                {        
                    transform.LookAt(g.transform.position);
                    transform.position = Vector3.MoveTowards(transform.position, g.transform.position, 0.2f);

                    g.transform.LookAt(transform.position);
                    g.transform.position = Vector3.MoveTowards(g.transform.position, transform.position, 0.2f);

                    if (distance <= 2f)
                    {
                        agent.speed = 0;
                        rb.velocity = Vector3.zero;

                        g.agent.speed = 0;
                        g.rb.velocity = Vector3.zero;

                        talkTimer = true;
                    }

                    if (talkTimer)
                    {
                        talkTime += Time.deltaTime;

                        if (talkTime >= 5f)
                        {
                            g.talking = false;
                            g.agent.speed = 4;

                            talking = false;
                            agent.speed = 3;
                            personRadius = 0;

                            talkTime = 0;
                            talkTimer = false;

                            talkCooldown = true;
                        }
                    }
                }
            }

            if (talkCooldown)
            {
                talkingCD += Time.deltaTime;

                if (talkingCD >= 50f)
                {
                    personRadius = 2.5f;
                    
                    talkingCD = 0f;
                    talkCooldown = false;
                }
            }
        }
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
