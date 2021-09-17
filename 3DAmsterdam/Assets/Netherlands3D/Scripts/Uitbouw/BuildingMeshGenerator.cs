using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.LayerSystem;
using ConvertCoordinates;

public struct Line
{
    public Vector3 Start;
    public Vector3 End;
    public Quaternion Rotation; //rotation with respect to the z axis

    public Line(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
        Rotation = Quaternion.FromToRotation(Vector3.forward, (end - start).normalized);
    }
}

namespace Netherlands3D.T3D.Uitbouw
{
    public class BuildingMeshGenerator : MonoBehaviour
    {
        public Vector3 BuildingCenter { get; private set; }
        public float GroundLevel { get; private set; }
        public bool IsMonument { get; private set; }
        //public Vector3[] RelativeBuildingCorners { get; private set; }
        public Vector3[] AbsoluteBuildingCorners { get; private set; }

        public bool BuildingDataIsProcessed { get; private set; } = false;

        public delegate void BuildingDataProcessedEventHandler(BuildingMeshGenerator building);
        public event BuildingDataProcessedEventHandler BuildingDataProcessed;

        private void Start()//in start to avoid race conditions
        {
            MetadataLoader.Instance.BuildingMetaDataLoaded += PerceelRenderer_BuildingMetaDataLoaded;
            MetadataLoader.Instance.BuildingOutlineLoaded += Instance_BuildingOutlineLoaded;
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
            BuildingDataIsProcessed = true;
        }

        private void Instance_BuildingOutlineLoaded(object source, BuildingOutlineEventArgs args)
        {
            StartCoroutine(ProcessCorners(args.Outline));
        }

        private IEnumerator ProcessCorners(List<Vector2[]> coords)
        {
            yield return new WaitUntil(() => BuildingDataIsProcessed); //wait until ground level is set

            var q = from i in coords
                    from p in i
                    select CoordConvert.RDtoUnity(p) into v3
                    select new Vector3(v3.x, GroundLevel, v3.z);

            print(q.Count());
            AbsoluteBuildingCorners = q.ToArray();
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

        /*public static Vector3[] EstimateBuildingCorners(Mesh mesh, float heightThreshold)
        {
            var verts = mesh.vertices;
            var tris = mesh.triangles;

            var corners = new List<Vector3>();
            var vertIndices = new List<int>();

            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 vert = verts[i];
                if (vert.y < heightThreshold)
                {
                    //corners.Add(vert);
                    vertIndices.Add(i);
                }
            }

            var lines = new List<Line>();
            var offSetIndices = new int[][] {
                new int[] { 1, 2 },
                new int[] { -1, 1 },
                new int[] { -2, -1 },
            };

            for (int i = 0; i < 3; i++)
            {
                for (int j = i; j < tris.Length; j += 3)
                {
                    var vertIndex = tris[j];
                    if (vertIndices.Contains(vertIndex))
                    {
                        // get the other 2 vert indices for this triangle
                        print("vertIndex" + vertIndex + "i: " + i + "j: " + j);
                        //print("A\t" + indexA + "\t" + offSetIndices[indexA] + "\t" + j + offSetIndices[indexA]);
                        //print("B\t" + indexB + "\t" + offSetIndices[indexB] + "\t" + j + offSetIndices[indexB]);

                        var vertIndexA = tris[j + offSetIndices[i][0]];
                        var vertIndexB = tris[j + offSetIndices[i][1]];

                        if (vertIndices.Contains(vertIndexA))
                        {
                            lines.Add(new Line(verts[vertIndex], verts[vertIndexA])); //todo: does direction matter?
                        }

                        if (vertIndices.Contains(vertIndexB))
                        {
                            lines.Add(new Line(verts[vertIndex], verts[vertIndexB])); //todo: does direction matter?
                        }
                    }
                }
            }

            foreach(Line l in lines)
            {
                Debug.DrawLine(l.Start, l.End, Color.green, 300f, false);
            }

            var startAngle = Quaternion.Angle(lines[0].Rotation, lines[lines.Count - 1].Rotation);
            if (startAngle > 5 && startAngle < 175)
            {
                //angle change steep enough, vert is probably a corner
                corners.Add(lines[0].Start);
            }

            for (int i = 0; i < lines.Count - 1; i++)
            {
                var newAngle = Quaternion.Angle(lines[i].Rotation, lines[i + 1].Rotation);
                var deltaAngle = Mathf.Abs(newAngle - startAngle);
                print("deltaAngle: " + deltaAngle);
                if (deltaAngle > 5 && deltaAngle < 175)
                {
                    //angle change steep enough, vert is probably a corner
                    corners.Add(lines[i].Start);
                }
            }

            return corners.ToArray();
        }
        */

        public Vector3 OffsetVertexPosition(Vector3 vertex)
        {
            return transform.rotation * vertex + transform.position;
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

        //private void OnDrawGizmos()
        //{
        //    foreach (var a in AbsoluteBuildingCorners)
        //    {
        //        Gizmos.color = Color.magenta;
        //        Gizmos.DrawCube(a, Vector3.one * 0.1f);
        //    }
        //}
    }
}
