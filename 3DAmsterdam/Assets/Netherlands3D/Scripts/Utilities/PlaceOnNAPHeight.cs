using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Utilities
{
    public class PlaceOnNAPHeight : MonoBehaviour
    {
        [SerializeField]
        private float NAP;

        void Start()
        {
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, NAP - CoordConvert.zeroGroundLevelY);
        }
    }
}