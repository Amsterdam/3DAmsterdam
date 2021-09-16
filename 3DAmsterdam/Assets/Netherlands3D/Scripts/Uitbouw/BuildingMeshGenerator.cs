using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.LayerSystem;

namespace Netherlands3D.T3D.Uitbouw
{
    public class BuildingMeshGenerator : MonoBehaviour
    {
        public Vector3 BuildingCenter { get; private set; }
        public float GroundLevel { get; private set; }
        public bool IsMonument { get; private set; }
        public Vector3[] BuildingCorners { get; private set; }

        public delegate void BuildingDataProcessedEventHandler(BuildingMeshGenerator building);
        public event BuildingDataProcessedEventHandler BuildingDataProcessed;

        private void Start()//in start to avoid race conditions
        {
            MetadataLoader.Instance.BuildingMetaDataLoaded += PerceelRenderer_BuildingMetaDataLoaded;
        }

        private void PerceelRenderer_BuildingMetaDataLoaded(object source, ObjectDataEventArgs args)
        {
            print("constructing mesh");
            var buildingMesh = ExtractBuildingMesh(args.ObjectData, args.ObjectData.highlightIDs[0]);

            transform.position = args.TileOffset;
            var mf = GetComponent<MeshFilter>();
            mf.mesh = buildingMesh;

            var col = gameObject.AddComponent<MeshCollider>();
            BuildingCenter = col.bounds.center;
            GroundLevel = BuildingCenter.y - col.bounds.extents.y; //hack: if the building geometry goes through the ground this will not work properly

            //IsMonument = args.ObjectData. //todo: where to get this data?

            BuildingDataProcessed.Invoke(this); // it cannot be assumed if the perceel or building data loads + processes first due to the server requests, so this event is called to make sure the processed building information can be used by other classes
        }

        public static Mesh ExtractBuildingMesh(ObjectData objectData, string id)
        {
            var idIndex = objectData.ids.IndexOf(id);

            List<int> vertIndices = new List<int>();
            for (int i = 0; i < objectData.vectorMap.Count; i++)
            {
                if (objectData.vectorMap[i] == idIndex)
                {
                    vertIndices.Add(i);
                }
            }

            //copy mesh data to avoid getting a copy every iteration in the loop
            var sourceVerts = objectData.mesh.vertices;
            var sourceTriangles = objectData.mesh.triangles;
            var sourceUVs = objectData.uvs;

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();

            List<int> usedVerts = new List<int>();
            for (int i = 0; i < sourceTriangles.Length; i += 3)
            {
                //check if the current triangle is part of the extracted verts
                if (vertIndices.Contains(sourceTriangles[i]))// || vertIndices.Contains(sourceTriangles[i + 1]) || vertIndices.Contains(sourceTriangles[i + 2]))
                {
                    //add matching triangle to my mesh
                    for (int j = 0; j < 3; j++)
                    {
                        //check if this vertex is already used
                        var existingVertIndex = usedVerts.FindIndex(x => x == sourceTriangles[i + j]);
                        int newTriIndex = -1;
                        if (existingVertIndex == -1) //vert not found, add this vert
                        {
                            vertices.Add(sourceVerts[sourceTriangles[i + j]]);
                            uvs.Add(sourceUVs[sourceTriangles[i + j]]);
                            newTriIndex = vertices.Count - 1;
                            usedVerts.Add(sourceTriangles[i + j]);
                        }
                        else //vert already in use, so just add the triangle index
                        {
                            newTriIndex = existingVertIndex;
                        }

                        triangles.Add(newTriIndex);
                    }
                }
            }

            //generate mesh from data
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }

        public static Vector3[] EstimateBuildingCorners(Mesh mesh, float heightThreshold)
        {
            var verts = mesh.vertices;
            var corners = new List<Vector3>();
            foreach (var vert in verts)
            {
                if(vert.y < heightThreshold)
                {
                    corners.Add(vert);
                }
            }

            return corners.ToArray();
        }

        //private void OnDrawGizmos()
        //{
        //    foreach (var vert in vertices)
        //    {
        //        Debug.DrawLine(Vector3.zero, vert + offset, Color.red);
        //        Gizmos.color = Color.red;
        //        Gizmos.DrawSphere(vert + offset, 0.1f);
        //    }
        //}
    }
}
