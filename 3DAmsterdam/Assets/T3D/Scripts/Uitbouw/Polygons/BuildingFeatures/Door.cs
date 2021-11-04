using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class Door : BoundaryFeature
    {
        protected override void Update()
        {
            base.Update();
            SnapToGround();
        }

        private void SnapToGround()
        {
            var vPos = Size.y / 2;
            transform.localPosition = new Vector3(transform.localPosition.x, vPos, transform.localPosition.z);
        }
    }
}
