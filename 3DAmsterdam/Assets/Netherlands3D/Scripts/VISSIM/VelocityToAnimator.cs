using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityToAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private Vector3 previousFramePosition = Vector3.zero;

    [SerializeField] private float multiplyVelocity = 10.0f;

    void FixedUpdate()
    {
        var velocity = Vector3.Distance(previousFramePosition, this.transform.position);
        animator.SetFloat("Speed", velocity * multiplyVelocity);

        previousFramePosition = this.transform.position;
    }
}
