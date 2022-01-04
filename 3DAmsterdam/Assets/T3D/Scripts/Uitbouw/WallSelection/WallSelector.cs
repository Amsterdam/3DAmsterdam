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

        private string rayOriginKey;
        private SaveableVector3 rayOrigin;
        private string rayDirectionKey;
        private SaveableVector3 rayDirection;

        private void Awake()
        {
            rayOriginKey = GetType().ToString() + ".rayOrigin";
            rayDirectionKey = GetType().ToString() + ".rayDirection";

            rayOrigin = new SaveableVector3(rayOriginKey);
            rayDirection = new SaveableVector3(rayDirectionKey);

            wallMeshFilter = GetComponent<MeshFilter>();
            building = GetComponentInParent<BuildingMeshGenerator>();
        }

        private void OnEnable()
        {
            building.BuildingDataProcessed += Building_BuildingDataProcessed;
        }

        private void OnDisable()
        {
            building.BuildingDataProcessed -= Building_BuildingDataProcessed;
        }

        private void Building_BuildingDataProcessed(BuildingMeshGenerator building)
        {
            if (SessionSaver.LoadPreviousSession)
            {
                var ray = new Ray(rayOrigin.Value, rayDirection.Value);
                if (TryGetValidWall(ray, out var wall))
                {
                    WallMesh = wall;
                    wallMeshFilter.mesh = WallMesh;
                    WallIsSelected = true;

                    //complete step after loading wall
                    //AllowSelection = false;
                    //MetadataLoader.Instance.PlaatsUitbouw();
                }
            }
        }

        void Update()
        {
            var mask = LayerMask.GetMask("ActiveSelection");

            if (AllowSelection && ObjectClickHandler.GetClickOnObject(false, mask))
            {
                if (EventSystem.current.IsPointerOverGameObject()) //clicked on ui elements
                    return;

                var ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
                if (TryGetValidWall(ray, out var wall))
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

        private bool TryGetValidWall(Ray ray, out Mesh face)
        {
            //try to get a face, check if this face is grounded and if this face is vertical
            return TryGetFace(ray, out face) && CheckIfGrounded(face, building.GroundLevel, groundLevelOffsetTolerance) && CheckIfVertical(face, verticalComponentTolerance);
        }

        private bool TryGetFace(Ray ray, out Mesh face)
        {
            face = new Mesh();

            rayOrigin.SetValue(ray.origin);
            rayDirection.SetValue(ray.direction);

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

                int[] clickedTriangle = new int[3];
                bool foundClickedTriangle = false;

                var test = new GameObject();
                test.transform.position = hit.point;

                for (int i = 0; i < sourceTriangles.Length; i += 3)
                {
                    Vector3 triangleNormal = CalculateNormal(sourceVerts[sourceTriangles[i]], sourceVerts[sourceTriangles[i + 1]], sourceVerts[sourceTriangles[i + 2]]);

                    if ((triangleNormal - hit.normal).sqrMagnitude < triangleNormalTolerance)
                    {
                        // parallel triangle, possibly part of the wall
                        // This tri is not part of the wall if it is not contiguous to other triangles

                        if (IsCoplanar(WallPlane, sourceVerts[sourceTriangles[i]] + transform.position, coplanarTolerance)) //only checks 1 vert, but due to the normal check we already filtered out unwanted tris that might have only 1 point in the zone of tolerance
                        {
                            // check if this is the clicked triangle to determine contiguous triangles later

                            var relativeHitPoint = hit.point - building.transform.position;
                                IsInsideTriangle(relativeHitPoint, sourceVerts[sourceTriangles[i]], sourceVerts[sourceTriangles[i + 1]], sourceVerts[sourceTriangles[i + 2]]);
                            //if (!foundClickedTriangle && IsInsideTriangle(hit.point, sourceVerts[sourceTriangles[i]], sourceVerts[sourceTriangles[i + 1]], sourceVerts[sourceTriangles[i + 2]]))
                            //{

                            //    var a = new GameObject();
                            //    a.transform.position = sourceVerts[sourceTriangles[i]];
                            //    var b = new GameObject();
                            //    b.transform.position = sourceVerts[sourceTriangles[i + 1]];
                            //    var c = new GameObject();
                            //    c.transform.position = sourceVerts[sourceTriangles[i + 2]];

                            //    clickedTriangle[0] = sourceTriangles[i];
                            //    clickedTriangle[1] = sourceTriangles[i + 1];
                            //    clickedTriangle[2] = sourceTriangles[i + 2];

                            //    foundClickedTriangle = true;
                            //}

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

                // we have gotten all coplanar and parrallel triangles, but they don't have to be contiguous to the clicked triangle.
                // 1. check if this is the clicked triangle
                // a. rotate triangle to have the normal face the z axis
                // b. check if hit point is in triangle

                // 2. get all contiguous triangles to the clicked triangle

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

        //public GameObject planeObj;
        //int iteration = 0;
        private bool IsInsideTriangle(Vector3 point, Vector3 ta, Vector3 tb, Vector3 tc)
        {
            var normal = CalculateNormal(ta, tb, tc);
            Plane pa = new Plane(ta, tb, ta + normal);
            Plane pb = new Plane(tb, tc, tb + normal);
            Plane pc = new Plane(tc, ta, tc + normal);

            bool a = pa.GetSide(point); //positive means it's on the normal's side, and thus out of the triangle
            bool b = pb.GetSide(point);
            bool c = pc.GetSide(point);

            //var testa = Instantiate(planeObj);
            //testa.transform.position = ta + building.transform.position;
            //testa.transform.up = pa.normal;
            //testa.name = iteration.ToString();

            //var testb = Instantiate(planeObj);
            //testb.transform.position = tb + building.transform.position;
            //testb.transform.up = pb.normal;
            //testb.name = iteration.ToString();

            //var testc = Instantiate(planeObj);
            //testc.transform.position = tc + building.transform.position;
            //testc.transform.up = pc.normal;
            //testc.name = iteration.ToString();

            //iteration++;
            //print(a + "\t" + b + "\t" + c);

            // point is inside the triangle if all three sides are negative (the normal is the positive side and thus outside)
            return !a && !b && !c;
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
