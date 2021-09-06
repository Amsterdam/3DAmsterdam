using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;


namespace Netherlands3D.LayerSystem
{
    public class BuildingMeshGenerator : MonoBehaviour
    {

        public static List<int> triangles = new List<int>();
        public static List<Vector3> vertices = new List<Vector3>();
        public static List<Vector2> uvs = new List<Vector2>();
        public static Vector3 offset;

        private void Start()//in start to avoid race conditions
        {
            PerceelRenderer.Instance.BuildingMetaDataLoaded += PerceelRenderer_BuildingMetaDataLoaded;
        }

        private void PerceelRenderer_BuildingMetaDataLoaded(object source, ObjectDataEventArgs args)
        {
            print("constructing mesh");

            offset = args.TileOffset;
            var buildingMesh = ExtractBuildingMesh(args.ObjectData, args.ObjectData.highlightIDs[0]);

            transform.position = args.TileOffset;
            var mf = GetComponent<MeshFilter>();
            mf.mesh = buildingMesh;
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

            vertices = new List<Vector3>();
            triangles = new List<int>();
            uvs = new List<Vector2>();

            List<int> usedVerts = new List<int>();
            for (int i = 0; i < sourceTriangles.Length; i += 3)
            {
                if (vertIndices.Contains(sourceTriangles[i]))// || vertIndices.Contains(sourceTriangles[i + 1]) || vertIndices.Contains(sourceTriangles[i + 2]))
                {
                    //add matching vert to my mesh
                    for (int j = 0; j < 3; j++)
                    {

                        var existingVertIndex = usedVerts.FindIndex(x => x == sourceTriangles[i + j]);
                        int newTriIndex = -1;
                        if (existingVertIndex == -1)
                        {
                            vertices.Add(sourceVerts[sourceTriangles[i + j]]);
                            uvs.Add(sourceUVs[sourceTriangles[i + j]]);
                            newTriIndex = vertices.Count - 1;
                            usedVerts.Add(sourceTriangles[i + j]);
                        }
                        else
                        {
                            newTriIndex = existingVertIndex;
                        }

                        triangles.Add(newTriIndex);
                    }
                }
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            return mesh;
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
