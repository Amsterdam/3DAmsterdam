using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.T3D.Uitbouw
{
    //todo: in case there are 2 coplanar walls, this script will treat them as one, need to add a check to ensure the returned wall is contiguous

    public class WallSelector : MonoBehaviour
    {
        [SerializeField]
        private MeshFilter buildingMeshFilter;
        private MeshFilter wallMeshFilter;

        [SerializeField]
        private float normalTolerance = 0.01f; //tolerance in difference between normals to still count as the same direction
        private float coplanarTolerance = 0.01f; //tolerance in difference click point and vertex position to the plane at that point

        public bool AllowSelection { get; set; }
        public bool WallIsSelected { get; private set; }
        public Plane WallPlane { get; private set; }
        public Mesh WallMesh { get; private set; }
        public Vector3 TopLeft { get; private set; }
        public Vector3 BottomRight { get; private set; }

        private void Awake()
        {
            wallMeshFilter = GetComponent<MeshFilter>();
        }

        // Update is called once per frame
        void Update()
        {
            print(WallIsSelected);
            if (AllowSelection && Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject()) //clicked on ui elements
                    return;

                if (TryGetWall(out var wall))
                {
                    WallMesh = wall;
                    wallMeshFilter.mesh = wall;
                    WallIsSelected = true;
                }
                else
                {
                    WallPlane = new Plane();
                    WallIsSelected = false;
                }
            }
        }

        private bool TryGetWall(out Mesh wall)
        {
            wall = new Mesh();
            var ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, LayerMask.GetMask("ActiveSelection")))
            {
                WallPlane = new Plane(hit.normal, hit.point);

                //copy mesh data to avoid getting a copy every iteration in the loop
                var sourceVerts = buildingMeshFilter.mesh.vertices;
                var sourceTriangles = buildingMeshFilter.mesh.triangles;
                //var sourceUVs = buildingMeshFilter.mesh.uvs;

                //List<Vector3> parallelVertices = new List<Vector3>();
                List<int> parallelTris = new List<int>();
                List<Vector3> parallelVertices = new List<Vector3>();
                List<int> usedVertIndices = new List<int>();

                for (int i = 0; i < sourceTriangles.Length; i += 3)
                {
                    Vector3 triangleNormal = CalculateNormal(sourceVerts[sourceTriangles[i]], sourceVerts[sourceTriangles[i + 1]], sourceVerts[sourceTriangles[i + 2]]);

                    if ((triangleNormal - hit.normal).sqrMagnitude < normalTolerance)
                    {
                        // parallel triangle, possibly part of the wall
                        // This tri is not part of the wall if it is not contiguous to other triangles

                        if (IsCoplanar(WallPlane, sourceVerts[sourceTriangles[i]] + transform.position, coplanarTolerance)) //only checks 1 vert, but due to the normal check we already filtered out unwanted tris that might have only 1 point in the zone of tolerance
                        {
                            for (int j = 0; j < 3; j++) //add the 3 verts as a triangle
                            {
                                var vertIndex = sourceTriangles[i + j]; //vertIndex in the old list of verts
                                if (!usedVertIndices.Contains(vertIndex))
                                {
                                    usedVertIndices.Add(vertIndex);
                                    parallelVertices.Add(sourceVerts[vertIndex]);
                                }
                                parallelTris.Add(usedVertIndices.IndexOf(vertIndex)); //add the index of the vert as it is in the new vertex list
                            }
                        }
                    }
                }

                wall.vertices = parallelVertices.ToArray();
                wall.triangles = parallelTris.ToArray();

                wall.RecalculateNormals();
                wall.RecalculateBounds();

                if (wall.triangles.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static Vector3 CalculateNormal(Vector3 trianglePointA, Vector3 trianglePointB, Vector3 trianglePointC)
        {
            var dir = Vector3.Cross(trianglePointB - trianglePointA, trianglePointC - trianglePointA);
            return dir.normalized;
        }

        private static bool IsCoplanar(Plane wallPlane, Vector3 point, float tolerance)
        {
            return Mathf.Abs(wallPlane.GetDistanceToPoint(point)) < tolerance;
        }
    }
}
