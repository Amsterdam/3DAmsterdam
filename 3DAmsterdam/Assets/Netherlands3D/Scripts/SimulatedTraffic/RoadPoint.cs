using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;

namespace Netherlands3D.Traffic
{
    public class RoadPoint : MonoBehaviour
    {
        public Vector3 pointCoordinates;
        public void Initiate(double longitude, double latitude)
        {
            Vector3 tempCoordinates = CoordConvert.WGS84toUnity(longitude, latitude);
            tempCoordinates.y = Config.activeConfiguration.zeroGroundLevelY;
            pointCoordinates = tempCoordinates;
            transform.position = pointCoordinates;
        }

        /// <summary>
        /// Draws this roads Gizmo's
        /// </summary>
        void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 1);
        }
    }
}