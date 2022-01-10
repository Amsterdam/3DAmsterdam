using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace Netherlands3D.T3D.Uitbouw
{
    public class UploadedUitbouw : UitbouwBase
    {
        public JSONNode CityObject { get; private set; }
        [SerializeField]
        private MeshFilter meshFilter;
        private Mesh mesh;

        Vector3 transformedExtents;

        public override Vector3 LeftCenter => meshFilter.transform.position + mesh.bounds.center - transform.right * transformedExtents.x;

        public override Vector3 RightCenter => meshFilter.transform.position + mesh.bounds.center + transform.right * transformedExtents.x;

        public override Vector3 TopCenter => meshFilter.transform.position + mesh.bounds.center + transform.up * transformedExtents.y;

        public override Vector3 BottomCenter => meshFilter.transform.position + mesh.bounds.center - transform.up * transformedExtents.y;

        public override Vector3 FrontCenter => meshFilter.transform.position + mesh.bounds.center - transform.forward * transformedExtents.z;

        public override Vector3 BackCenter => meshFilter.transform.position + mesh.bounds.center + transform.forward * transformedExtents.z;

        public override void UpdateDimensions()
        {
            SetDimensions(Multiply(meshFilter.transform.lossyScale, mesh.bounds.size));
        }

        public void SetMeshFilter(MeshFilter mf)
        {
            meshFilter = mf;
            mesh = meshFilter.mesh;
            transformedExtents = Multiply(meshFilter.transform.lossyScale, mesh.bounds.extents);
            UpdateDimensions();
        }

        public static Vector3 Multiply(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
    }
}