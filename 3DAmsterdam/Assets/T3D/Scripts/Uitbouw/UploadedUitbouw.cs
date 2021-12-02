using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace Netherlands3D.T3D.Uitbouw
{
    public class UploadedUitbouw : UitbouwBase
    {
        public JSONNode CityObject { get; private set; }

        public override Vector3 LeftCenter => throw new System.NotImplementedException();

        public override Vector3 RightCenter => throw new System.NotImplementedException();

        public override Vector3 TopCenter => throw new System.NotImplementedException();

        public override Vector3 BottomCenter => throw new System.NotImplementedException();

        public override Vector3 FrontCenter => throw new System.NotImplementedException();

        public override Vector3 BackCenter => throw new System.NotImplementedException();

        public override void UpdateDimensions()
        {
            throw new System.NotImplementedException();
        }
    }
}