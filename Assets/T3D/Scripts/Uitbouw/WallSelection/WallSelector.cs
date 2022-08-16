using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.T3D.Uitbouw
{
    //todo: in case there are 2 coplanar walls, this script will treat them as one, need to add a check to ensure the returned wall is contiguous

    public struct Edge
    {
        public int IndexA;
        public int IndexB;

        public Edge(int indexA, int indexB)
        {
            IndexA = indexA;
            IndexB = indexB;
        }

        public static bool operator ==(Edge edgeA, Edge edgeB)
        {
            bool sameDirection = (edgeA.IndexA == edgeB.IndexA) && (edgeA.IndexB == edgeB.IndexB);
            bool reverseDirection = (edgeA.IndexA == edgeB.IndexB) && (edgeA.IndexB == edgeB.IndexA);

            return sameDirection || reverseDirection;
        }

        public static bool operator !=(Edge edgeA, Edge edgeB)
        {
            bool sameDirection = (edgeA.IndexA == edgeB.IndexA) && (edgeA.IndexB == edgeB.IndexB);
            bool reverseDirection = (edgeA.IndexA == edgeB.IndexB) && (edgeA.IndexB == edgeB.IndexA);

            return !sameDirection && !reverseDirection;
        }

        public override bool Equals(object obj)
        {
            return obj is Edge && (Edge)obj == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class WallSelectorSaveDataContainer : SaveDataContainer
    {
        public Vector3 RayOrigin;
        public Vector3 RayDirection;
    }

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
        public bool WallChanged { get; set; } //is a different wall selected than before? Used to reposition the uitbouw when user went back and selected a different wall

        private WallSelectorSaveDataContainer saveData;

        private void Awake()
        {
            saveData = new WallSelectorSaveDataContainer();

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
                LoadSelectedWall();
            }
        }

        private void LoadSelectedWall()
        {
            var ray = new Ray(saveData.RayOrigin, saveData.RayDirection);
            if (TryGetValidWall(ray, out var wall))
            {
                WallMesh = wall;
                wallMeshFilter.mesh = WallMesh;
                WallIsSelected = true;

                //complete step after loading wall
                //AllowSelection = false;
                //ServiceLocator.GetService<MetadataLoader>().PlaatsUitbouw();
            }
        }

        void Update()
        {
            var mask = LayerMask.GetMask("ActiveSelection");

            if (AllowSelection && ObjectClickHandler.GetClickOnObject(false, mask))
            {
                if (EventSystem.current.IsPointerOverGameObject()) //clicked on ui elements
                    return;

                var ray = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenPointToRay(Input.mousePosition);
                if (TryGetValidWall(ray, out var wall))
                {
                    WallMesh = wall;
                    wallMeshFilter.mesh = WallMesh;
                    WallIsSelected = true;
                    WallChanged = true;
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

            saveData.RayOrigin = ray.origin;
            saveData.RayDirection = ray.direction;

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("ActiveSelection")))
            {
                WallPlane = new Plane(hit.normal, hit.point);

                //copy mesh data to avoid getting a copy every iteration in the loop
                var sourceVerts = buildingMeshFilter.mesh.vertices;
                var sourceTriangles = buildingMeshFilter.mesh.triangles;
                //var sourceUVs = buildingMeshFilter.mesh.uvs;

                //List<Vector3> parallelVertices = new List<Vector3>();
                List<int> coplanarTriangles = new List<int>();
                List<Vector3> coplanarVertices = new List<Vector3>();
                List<int> usedVertIndices = new List<int>();

                List<Edge> edges = new List<Edge>();

                //int[] clickedTriangle = new int[3];
                List<Edge> contiguousTriEdges = new List<Edge>();
                bool foundClickedTriangle = false;
                List<int> contiguousTris = new List<int>();

                for (int i = 0; i < sourceTriangles.Length; i += 3)
                {
                    Vector3 triangleNormal = CalculateNormal(sourceVerts[sourceTriangles[i]], sourceVerts[sourceTriangles[i + 1]], sourceVerts[sourceTriangles[i + 2]]);

                    if ((triangleNormal - hit.normal).sqrMagnitude < triangleNormalTolerance)
                    {
                        // parallel triangle, possibly part of the wall
                        // This tri is not part of the wall if it is not contiguous to other triangles, in ordet to check that we save the edges as well as the verts for later use
                        if (IsCoplanar(WallPlane, sourceVerts[sourceTriangles[i]] + transform.position, coplanarTolerance)) //only checks 1 vert, but due to the normal check we already filtered out unwanted tris that might have only 1 point in the zone of tolerance
                        {
                            for (int j = 0; j < 3; j++) //add the 3 verts as a triangle
                            {
                                var vertIndex = sourceTriangles[i + j]; //vertIndex in the old list of verts
                                if (!usedVertIndices.Contains(vertIndex))
                                {
                                    usedVertIndices.Add(vertIndex);
                                    coplanarVertices.Add(sourceVerts[vertIndex]);

                                }
                                coplanarTriangles.Add(usedVertIndices.IndexOf(vertIndex)); //add the index of the vert as it is in the new vertex list

                                edges.Add(new Edge(vertIndex, sourceTriangles[i + (j + 1) % 3])); // add the edges to find shared edges later. These edges use the original indexing, so this needs to be converted
                            }

                            //re-index the edges of this triangle this is a separate loop because the new indexes of j=1 and/or j=2 do not exist yet in the previous loop
                            int triangleStartIndex = edges.Count - 3;
                            for (int j = 0; j < 3; j++)
                            {
                                var edge = edges[triangleStartIndex + j];
                                var newIndexA = usedVertIndices.IndexOf(edge.IndexA);
                                var newIndexB = usedVertIndices.IndexOf(edge.IndexB);

                                edges[triangleStartIndex + j] = new Edge(newIndexA, newIndexB);
                            }

                            // check if this is the clicked triangle to determine contiguous triangles later
                            var relativeHitPoint = hit.point - building.transform.position;
                            if (!foundClickedTriangle && IsInsideTriangle(relativeHitPoint, sourceVerts[sourceTriangles[i]], sourceVerts[sourceTriangles[i + 1]], sourceVerts[sourceTriangles[i + 2]]))
                            {
                                //if this is the clicked triangle, the last triangle added to the coplanar triangle list is the one that should be used as start to find the contiguous triangles.
                                var a = coplanarTriangles[coplanarTriangles.Count - 3];
                                var b = coplanarTriangles[coplanarTriangles.Count - 2];
                                var c = coplanarTriangles[coplanarTriangles.Count - 1];

                                contiguousTriEdges.Add(new Edge(a, b));
                                contiguousTriEdges.Add(new Edge(b, c));
                                contiguousTriEdges.Add(new Edge(c, a));

                                contiguousTris.Add(a);
                                contiguousTris.Add(b);
                                contiguousTris.Add(c);

                                foundClickedTriangle = true;
                            }
                        }
                    }
                }

                // we have gotten all coplanar and parrallel triangles, but they don't have to be contiguous to the clicked triangle. We also have the clicked triangle
                // get all contiguous triangles to the clicked triangle

                //for each coplanar triangle
                for (int i = edges.Count - 1; i >= 0; i -= 3) // go backwards so the end condition can stay the same
                {
                    for (int j = 2; j >= 0; j--)
                    {
                        //if this edge is already in the list, add the edges of this triangle
                        if (contiguousTriEdges.Contains(edges[i - j]))
                        {
                            contiguousTriEdges.Add(edges[i - 2]);
                            contiguousTriEdges.Add(edges[i - 1]);
                            contiguousTriEdges.Add(edges[i]);

                            // save these indices
                            contiguousTris.Add(coplanarTriangles[i - 2]);
                            contiguousTris.Add(coplanarTriangles[i - 1]);
                            contiguousTris.Add(coplanarTriangles[i]);

                            //if one edge matches, the triangle is added, it could be that a previously skipped triangle is contiguous to the newly added triangle, so the outer loop should reset.                             
                            //remove the found triangle edges so it is not tested again, and an infinite loop is avoided.
                            edges.RemoveAt(i); //order matters to avoid indexing problems
                            edges.RemoveAt(i - 1);
                            edges.RemoveAt(i - 2);
                            coplanarTriangles.RemoveAt(i);
                            coplanarTriangles.RemoveAt(i - 1);
                            coplanarTriangles.RemoveAt(i - 2);

                            //reset outer loop
                            i = edges.Count - 1 + 3; //add 3 because the for loop will subtract 3 after restarting
                            //if one edge matches, the remaining 1 or 2 don't need to be tested
                            break;
                        }
                    }
                }

                //remove unused vertices.
                //This algorithm doesn't scale well because it contains 2 loops through the tri list each iteration (with the Contains() function and the j loop), but it should not be a problem for our purposes
                for (int i = 0; i < coplanarVertices.Count; i++)
                {
                    if (contiguousTris.Contains(i))
                    {
                        continue;
                    }
                    else
                    {
                        coplanarVertices.RemoveAt(i);
                        for (int j = 0; j < contiguousTris.Count; j++)
                        {
                            if (contiguousTris[j] > i)
                            {
                                contiguousTris[j]--;
                            }
                        }
                    }
                }

                face.vertices = coplanarVertices.ToArray();
                face.triangles = contiguousTris.ToArray();

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

        private bool IsInsideTriangle(Vector3 point, Vector3 ta, Vector3 tb, Vector3 tc)
        {
            var normal = CalculateNormal(ta, tb, tc);
            Plane pa = new Plane(ta, tb, ta + normal);
            Plane pb = new Plane(tb, tc, tb + normal);
            Plane pc = new Plane(tc, ta, tc + normal);

            bool a = pa.GetSide(point); //positive means it's on the normal's side, and thus out of the triangle
            bool b = pb.GetSide(point);
            bool c = pc.GetSide(point);

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
