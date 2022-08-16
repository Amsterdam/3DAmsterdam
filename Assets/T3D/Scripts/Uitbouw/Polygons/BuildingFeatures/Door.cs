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
            var vPos = Size.y / 2 +  0.03f; //ugly hack to add 3cm, but it's to ensure the CityJson hole polygon does not glitch out/give an error when the height is exactly on the boundary line
            transform.localPosition = new Vector3(transform.localPosition.x, vPos, transform.localPosition.z);
        }
    }
}
