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
            featureTransform.localPosition = new Vector3(featureTransform.localPosition.x, vPos, featureTransform.localPosition.z);
        }
    }
}
