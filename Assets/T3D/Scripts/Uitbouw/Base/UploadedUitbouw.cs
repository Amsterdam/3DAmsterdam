using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using T3D.Uitbouw;
using System;

namespace Netherlands3D.T3D.Uitbouw
{
    public class UploadedUitbouw : UitbouwBase
    {
        private List<CityObject> cityObjects = new List<CityObject>();
        [SerializeField]
        private MeshFilter meshFilter;
        public MeshFilter MeshFilter => meshFilter;
        private Mesh mesh;

        //private MeshFilter tempMeshFilter;

        Vector3 transformedExtents;

        public override Vector3 LeftCenter => meshFilter.transform.position + transform.rotation * mesh.bounds.center - transform.right * transformedExtents.x;

        public override Vector3 RightCenter => meshFilter.transform.position + transform.rotation * mesh.bounds.center + transform.right * transformedExtents.x;

        public override Vector3 TopCenter => meshFilter.transform.position + transform.rotation * mesh.bounds.center + transform.up * transformedExtents.y;

        public override Vector3 BottomCenter => meshFilter.transform.position + transform.rotation * mesh.bounds.center - transform.up * transformedExtents.y;

        public override Vector3 FrontCenter => meshFilter.transform.position + transform.rotation * mesh.bounds.center - transform.forward * transformedExtents.z;

        public override Vector3 BackCenter => meshFilter.transform.position + transform.rotation * mesh.bounds.center + transform.forward * transformedExtents.z;

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
            GetComponentInChildren<MeshCollider>().sharedMesh = meshFilter.mesh;
        }

        public static Vector3 Multiply(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public void AddCityObject(CityJSONToCityObject newCityObject)
        {
            if (cityObjects.Contains(newCityObject))
            {
                Debug.LogError("list already contains this City object");
                return;
            }

            cityObjects.Add(newCityObject);
            newCityObject.Type = CityObjectType.BuildingPart;
        }

        public void UnparentFromMainBuilding()
        {
            foreach(var co in cityObjects)
            {
                co.SetParents(new CityObject[0]);
                co.Type = CityObjectType.Building;
            }
        }

        public void ReparentToMainBuilding(CityObject mainBuilding)
        {
            foreach(var co in cityObjects)
            {
                co.SetParents(new CityObject[] { mainBuilding });
                co.Type = CityObjectType.BuildingPart;
            }
        }
    }
}