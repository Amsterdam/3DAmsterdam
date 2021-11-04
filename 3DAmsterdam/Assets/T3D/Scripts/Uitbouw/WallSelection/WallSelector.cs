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

        private BuildingMeshGenerator building;

        [SerializeField]
        private float triangleNormalTolerance = 0.01f; //tolerance in difference between normals to still count as the same direction
        private float coplanarTolerance = 0.01f; //tolerance in difference click point and vertex position to the plane at that point
        [SerializeField]
        private float verticalComponentTolerance = 0.1f; //tolerance in the y component of the wall's normal before it is being seen as not vertical
        [SerializeField]
        private float groundLevelOffsetTolerance = 0.05f; //tolerance of how much the wall may be off the ground to still count as a ground touching wall

        public bool AllowSelection { get; set; }
        public bool WallIsSelected { get; private set; }
        public Plane WallPlane { get; private set; }
        public Mesh WallMesh { get; private set; }
        public Vector3 CenterPoint { get; private set; }

        private void Awake()
        {
            wallMeshFilter = GetComponent<MeshFilter>();
            building = GetComponentInParent<BuildingMeshGenerator>();
        }

        // Update is called once per frame
        void Update()
        {
            var mask = LayerMask.GetMask("ActiveSelection");

            if (AllowSelection && ObjectClickHandler.GetClickOnObject(false, mask))
            {
                if (EventSystem.current.IsPointerOverGameObject()) //clicked on ui elements
                    return;

                if (TryGetValidWall(out var wall))
                {
                    WallMesh = wall;
                    wallMeshFilter.mesh = WallMesh;
                    WallIsSelected = true;
                }
                else
                {
                    WallMesh = new Mesh();
                    wallMeshFilter.mesh = WallMesh;
                    WallIsSelected = false;
                    WallPlane = new Plane();
                }
            }
        }

        private bool TryGetValidWall(out Mesh face)
        {
            //try to get a face, check if this face is grounded and if this face is vertical
            return TryGetFace(out face) && CheckIfGrounded(face, building.GroundLevel, groundLevelOffsetTolerance) && CheckIfVertical(face, verticalComponentTolerance);
        }

        private bool TryGetFace(out Mesh face)
        {
            face = new Mesh();
            var ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("ActiveSelection")))
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

                    if ((triangleNormal - hit.normal).sqrMagnitude < triangleNormalTolerance)
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

                face.vertices = parallelVertices.ToArray();
                face.triangles = parallelTris.ToArray();

                face.RecalculateNormals();
                face.RecalculateBounds();

                if (face.triangles.Length > 0)
                {
                    CenterPoint = transform.position + face.bounds.center;

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


        private static bool CheckIfVertical(Mesh face, float tolerance)
        {
            // this method assumes a coplanar set of triangles, so if one of them is vertical, they all should be.

            if (face.triangles.Length >= 3)
            {
                var normal = CalculateNormal(face.vertices[face.triangles[0]],
                                             face.vertices[face.triangles[1]],
                                             face.vertices[face.triangles[2]]);

                return normal.y < tolerance;
            }
            return false;
        }

        private static bool CheckIfGrounded(Mesh face, float groundLevel, float offsetTolerance)
        {
            return Mathf.Abs(face.bounds.min.y - groundLevel) < offsetTolerance;
        }
    }
}
