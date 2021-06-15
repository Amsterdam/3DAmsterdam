using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic.VISSIM
{

    public class Pedestrian : VissimVehicle
    {
        [SerializeField] private Animator pedestrianAnimator = default;
        protected override void Awake()
        {
            base.Awake();
            pedestrianAnimator = transform.GetChild(0).GetComponent<Animator>();
        }

        // adds a walking animation to the pedestrian characters and only gets activated when its time for the pedestrian to move
        public override void MoveAnimation(Vector3 startPos, Vector3 endPos, float animationTime)
        {
            base.MoveAnimation(startPos, endPos, animationTime);

            if (Vector3.Distance(startPos, endPos) > 0.1f)
            {
                pedestrianAnimator.SetBool("isWalking", true);
            }
            else
            {
                pedestrianAnimator.SetBool("isWalking", false);
            }
        }
    }
}