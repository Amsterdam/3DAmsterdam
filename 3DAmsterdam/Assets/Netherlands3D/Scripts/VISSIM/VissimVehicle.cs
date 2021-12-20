using Netherlands3D.TileSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic.VISSIM
{
    public class VissimVehicle : MonoBehaviour
    {
        public VissimData vehicleCommandData;
        public RaycastHit hit;
        protected Vector3 startLastRecordedHeight = default;
        protected Vector3 endLastRecordedHeight = default;

        public Vector3 futurePosition = default;

        protected Animation anim = default;

        protected float minimumDistanceForAnimation = 0.1f;

        protected float timeSinceCommand = 0.0f;
        protected float cleanUpTime = 5;

        protected BinaryMeshLayer meshLayer;
        protected VehicleProperties vehicleProperties;

        protected virtual void Awake()
        {
            anim = GetComponent<Animation>();
            timeSinceCommand = Time.time + cleanUpTime;
            vehicleProperties = gameObject.GetComponent<VehicleProperties>();
        }

        protected virtual void Update()
        {
            // if the car hasn't been used, it will be disabled and hidden from scene view
            if (Time.time > timeSinceCommand && gameObject.activeSelf)
            {
                CleanUpVehicle();
            }
        }

        private void LateUpdate()
        {
            if (vehicleProperties != null)
            {
                float degrees = 5.0f;
                Quaternion targetRotation = new Quaternion(vehicleProperties.GetNewRotation(transform.rotation).x,transform.rotation.y, vehicleProperties.GetNewRotation(transform.rotation).z,transform.rotation.w); 
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, degrees * Time.deltaTime);
            }
        }

        /// <summary>
        /// Moves the vehicle through animation.
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="animationTime"></param>
        public virtual void MoveAnimation(Vector3 startPos, Vector3 endPos, float animationTime)
        {
            timeSinceCommand = Time.time + cleanUpTime;
            // checks for the height and places the car on the correct position on the map (maaiveld)
            Vector3 temp = transform.position;
            temp.y = 50f;

            if (Physics.Raycast(temp, -Vector3.up, out hit, Mathf.Infinity))
            {
                startLastRecordedHeight = hit.point;
                vehicleCommandData.coordFront.y = hit.point.y;
            }
            else
            {
                meshLayer.AddMeshColliders(hit.point);
            }
            // checks the positional height of there the car will be in the future/next simulation second
            temp = endPos;
            temp.y = 50f;
            if (Physics.Raycast(temp, -Vector3.up, out hit, Mathf.Infinity))
            {
                endLastRecordedHeight = hit.point;
            }
            else
            {
                meshLayer.AddMeshColliders(hit.point);
            }

            vehicleCommandData.coordFront.y = startLastRecordedHeight.y;


            // create a new AnimationClip
            AnimationClip clip = new AnimationClip();
            clip.legacy = true;

            Vector3 _direction = (endPos - vehicleCommandData.coordRear).normalized;
            // movement aniomation
            clip.SetCurve("", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0.0f, startPos.x, animationTime, endPos.x)); // sets linear curve from the start position of the car and the end
            clip.SetCurve("", typeof(Transform), "localPosition.y", AnimationCurve.Linear(0.0f, startLastRecordedHeight.y, animationTime, endLastRecordedHeight.y)); // Misschien endlastrecordheight?
            clip.SetCurve("", typeof(Transform), "localPosition.z", AnimationCurve.Linear(0.0f, startPos.z, animationTime, endPos.z));

            //Should the car even rotate? If the next position is too close then it won't rotate.
            if (transform.rotation.y == 0)
            {
                transform.rotation = Quaternion.LookRotation((futurePosition - vehicleCommandData.coordRear).normalized);
            }
            // rotation animation
            if (Vector3.Distance(startPos, endPos) > minimumDistanceForAnimation)
            {

                //clip.SetCurve("", typeof(Transform), "localRotation.x", AnimationCurve.Linear(0.0f, transform.rotation.x, animationTime, Quaternion.LookRotation(_direction).x)); //convert to quaternions
                clip.SetCurve("", typeof(Transform), "localRotation.y", AnimationCurve.Linear(0.0f, transform.rotation.y, animationTime, Quaternion.LookRotation(_direction).y)); //convert to quaternions
                //clip.SetCurve("", typeof(Transform), "localRotation.z", AnimationCurve.Linear(0.0f, transform.rotation.z, animationTime, Quaternion.LookRotation(_direction).z)); //convert to quaternions
                clip.SetCurve("", typeof(Transform), "localRotation.w", AnimationCurve.Linear(0.0f, transform.rotation.w, animationTime, Quaternion.LookRotation(_direction).w)); //convert to quaternions 
  
            }

            // Plays the animated clip of the vehicle
            anim.AddClip(clip, clip.name);
            anim.Play(clip.name);

        }

        /// <summary>
        /// Removes the vehicle when its inactive.
        /// </summary>
        protected virtual void CleanUpVehicle()
        {
            this.gameObject.SetActive(false);
        }

        public virtual void SetMeshLayer(BinaryMeshLayer layer)
        {
            meshLayer = layer;
        }
    }
}