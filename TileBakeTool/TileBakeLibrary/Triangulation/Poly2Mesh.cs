//	Poly2Mesh
//
// author: Joe Strout (http://luminaryapps.com/blog/triangulating-3d-polygons-in-unity/)
//	This is a static class that wraps up all the details of creating a Mesh
//	(or even an entire GameObject) out of a polygon.  The polygon must be
//	planar, and should be well-behaved (no duplicate points, etc.), but it
//	can have any number of non-overlapping holes.  In addition, the polygon
//	can be in ANY plane -- it doesn't have to be the XY plane.  Huzzah!
//
//	To use:
//		1. Create a Poly2Mesh.Polygon.
//		2. Set its .outside to a list of Vector3's describing the outside of the shape.
//		3. [Optional] Add to its .holes list as desired.
//		4. [Optional] Call CalcPlaneNormal on it, passing in a hint as to which way you
//			want the polygon to face.
//		5. Pass it to Poly2Mesh.CreateMesh, or Poly2Mesh.CreateGameObject.
//  
// 11-10-2021 - Option to add thickness to poly was added by 3D Amsterdam
// 11-10-2021 - Added namespace to guard from conflicts in Unity3D by 3D Amsterdam

using Poly2Tri;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace JoeStrout
{
    public static class Poly2Mesh
    {
        // Polygon: defines the input to the triangulation algorithm.
        public class Polygon
        {
            // outside: a set of points defining the outside of the polygon
            public List<Vector3> outside;

            // holes: a (possibly empty) list of non-overlapping holes within the polygon
            public List<List<Vector3>> holes;

            // Optional UV lists, which go in parallel to the Vector3 positions above
            public List<Vector2> outsideUVs;
            public List<List<Vector2>> holesUVs;

            // normal to the plane containing the polygon (normally calculated by CalcPlaneNormal)
            public Vector3 planeNormal;

            // rotation into the XY plane (normally calculated by CalcRotation)
            public Quaternion rotation = Quaternion.Identity;

            // constructor (just initializes the lists)
            public Polygon()
            {
                outside = new List<Vector3>();
                holes = new List<List<Vector3>>();
            }

            /// <summary>
            /// Calculates the plane normal for this polygon (with no flipping).
            /// Assumes a clockwise winding order.
            /// </summary>
            public void CalcPlaneNormal()
            {
                // Calculate the plane normal by summing the cross product of
                // all points relative to the start/end point, and normalizing.
                // Reference: http://stackoverflow.com/questions/32274127

                Vector3 basePt = outside[0];
                Vector3 sum = Vector3.Zero;
                for (int i = 1; i < outside.Count - 1; i++)
                {
                    if (outside[i] == basePt || outside[i + 1] == basePt) continue;
                    sum += Vector3.Cross(outside[i] - basePt, outside[i + 1] - basePt);
                }
                // Now, sum is a vector that points in the normal direction with proper
                // orientation, and its magnitude is 2 times the polygon area.  Neat, huh?
                planeNormal = Vector3.Normalize(sum);
            }

            /// <summary>
            /// Calculates the plane normal for this polygon, trying to face the given direction.
            /// </summary>
            /// <param name="hint">Direction which you'd like the polygon to face as closely as possible.</param>
            public void CalcPlaneNormal(Vector3 hint)
            {
                planeNormal = Vector3.Cross(outside[1] - outside[0], outside[2] - outside[0]);
                planeNormal = Vector3.Normalize(planeNormal);
                if (GetAngleBetween(planeNormal, hint) > GetAngleBetween(-planeNormal, hint))
                {
                    planeNormal = -planeNormal;
                }
            }

            /// <summary>
            /// Calculates the rotation needed to get this polygon into the XY plane.
            /// </summary>
            public void CalcRotation()
            {
                if (planeNormal == Vector3.Zero) CalcPlaneNormal();
                if (planeNormal == Vector3.UnitZ)
                {
                    // Special case: our polygon is already in the XY plane, no rotation needed.
                    rotation = Quaternion.Identity;
                }
                else
                {
                    //TODO, add alternative for FromToRotation
                    rotation = FromToRotation(planeNormal, Vector3.UnitZ);
                }
            }

            static public Quaternion FromToRotation(Vector3 dir1, Vector3 dir2)
            {
                float r = 1f + Vector3.Dot(dir1, dir2);
                Vector3 w;
                if (r < 1E-6f)
                {
                    r = 0f;
                    w = Math.Abs(dir1.X) > Math.Abs(dir1.Z) ? new Vector3(-dir1.Y, dir1.Z, 0f) :
                                                               new Vector3(0f, -dir1.Z, dir1.Y);
                }
                else
                {
                    w = Vector3.Cross(dir1, dir2);
                }

                return Quaternion.Normalize(new Quaternion(w.X, w.Y, w.Z, r));
            }

            public Vector2 ClosestUV(Vector3 pos)
            {
                Vector2 bestUV = outsideUVs[0];
                float bestDSqr = (outside[0] - pos).LengthSquared();
                for (int i = 1; i < outsideUVs.Count; i++)
                {
                    float dsqr = (outside[i] - pos).LengthSquared();
                    if (dsqr < bestDSqr)
                    {
                        bestDSqr = dsqr;
                        bestUV = outsideUVs[i];
                    }
                }
                for (int h = 0; h < holes.Count; h++)
                {
                    List<Vector3> hole = holes[h];
                    List<Vector2> holeUVs = holesUVs[h];
                    for (int i = 0; i < holeUVs.Count; i++)
                    {
                        float dsqr = (hole[i] - pos).LengthSquared();
                        if (dsqr < bestDSqr)
                        {
                            bestDSqr = dsqr;
                            bestUV = holeUVs[i];
                        }
                    }
                }
                return bestUV;
            }
        }

        public static double GetAngleBetween(Vector3 a, Vector3 b)
        {
            var dot = Vector3.Dot(a, b);
            // Divide the dot by the product of the magnitudes of the vectors
            dot /= (a.Length() * b.Length());
            //Get the arc cosin of the angle, you now have your angle in radians 
            var acos = Math.Acos(dot);
            //Multiply by 180/Mathf.PI to convert to degrees
            var angle = acos * 180 / Math.PI;

            return angle;
        }

        public static Vector3 MultiplyQuaternionByVector(Quaternion quat, Vector3 vec)
        {
            float num = quat.X * 2f;
            float num2 = quat.Y * 2f;
            float num3 = quat.Z * 2f;
            float num4 = quat.X * num;
            float num5 = quat.Y * num2;
            float num6 = quat.Z * num3;
            float num7 = quat.X * num2;
            float num8 = quat.X * num3;
            float num9 = quat.Y * num3;
            float num10 = quat.W * num;
            float num11 = quat.W * num2;
            float num12 = quat.W * num3;
            Vector3 result;
            result.X = (1f - (num5 + num6)) * vec.X + (num7 - num12) * vec.Y + (num8 + num11) * vec.Z;
            result.Y = (num7 + num12) * vec.X + (1f - (num4 + num6)) * vec.Y + (num9 - num10) * vec.Z;
            result.Z = (num8 - num11) * vec.X + (num9 + num10) * vec.Y + (1f - (num4 + num5)) * vec.Z;
            return result;
        }

        /// <summary>
        /// Helper method to convert a set of 3D points into the 2D polygon points
        /// needed by the Poly2Tri code.
        /// </summary>
        /// <returns>List of 2D PolygonPoints.</returns>
        /// <param name="points">3D points.</param>
        /// <param name="rotation">Rotation needed to convert 3D points into the XY plane.</param>
        /// <param name="name="codeToPosition">Map (which we'll update) of PolygonPoint vertex codes to original 3D position.</param> 
        public static List<PolygonPoint> ConvertPoints(List<Vector3> points, Quaternion rotation, Dictionary<uint, Vector3> codeToPosition)
        {
            int count = points.Count;
            List<PolygonPoint> result = new List<PolygonPoint>(count);
            for (int i = 0; i < count; i++)
            {
                Vector3 originalPos = points[i];
                Vector3 p = MultiplyQuaternionByVector(rotation,originalPos);
                PolygonPoint pp = new PolygonPoint(p.X, p.Y);
                //			Debug.Log("Rotated " + originalPos.ToString("F4") + " to " + p.ToString("F4"));
                codeToPosition[pp.VertexCode] = originalPos;
                result.Add(pp);
            }
            return result;
        }

        /// <summary>
        /// Create a Mesh from a given Polygon.
        /// </summary>
        /// <returns>The freshly minted mesh.</returns>
        /// <param name="polygon">Polygon you want to triangulate.</param>
        public static void CreateMeshData(Polygon polygon, out Vector3[] vertices, out Vector3[] normals, out int[] triangles, out Vector2[] uvs, float thickness = 0)
        {
            //make sure we out something in case of failure
            vertices = new Vector3[0];
            normals = new Vector3[0];
            triangles = new int[0];
            uvs = null;

            // Ensure we have the rotation properly calculated, and have a valid normal
            if (polygon.rotation == Quaternion.Identity) polygon.CalcRotation();
            if (polygon.planeNormal == Vector3.Zero) return;       // bad data

            // Check for the easy case (a triangle)
            if (polygon.holes.Count == 0 && (polygon.outside.Count == 3 || (polygon.outside.Count == 4 && polygon.outside[3] == polygon.outside[0])))
            {
                CreateTriangle(polygon, out vertices, out normals, out triangles, out uvs);
                return;
            }
                                    
            // Rotate 1 point and note where it ends up in Z
            float z = MultiplyQuaternionByVector(polygon.rotation,polygon.outside[0]).Z;
            // Prepare a map from vertex codes to 3D positions.
            Dictionary<uint, Vector3> codeToPosition = new Dictionary<uint, Vector3>();
            // Convert the outside points (throwing out Z at this point)
            Poly2Tri.Polygon poly = new Poly2Tri.Polygon(ConvertPoints(polygon.outside, polygon.rotation, codeToPosition));
            // Convert each of the holes
            foreach (List<Vector3> hole in polygon.holes)
            {
                poly.AddHole(new Poly2Tri.Polygon(ConvertPoints(hole, polygon.rotation, codeToPosition)));
            }
            // Triangulate it!  Note that this may throw an exception if the data is bogus.
            try
            {
                DTSweepContext tcx = new DTSweepContext();
                tcx.PrepareTriangulation(poly);
                DTSweep.Triangulate(tcx);
                tcx = null;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            // Now, to get back to our original positions, use our code-to-position map.  We do
            // this instead of un-rotating to be a little more robust about noncoplanar polygons.
            // Create the Vector3 vertices (undoing the rotation),
            // and also build a map of vertex codes to indices
            Quaternion? invRot = null;
            Dictionary<uint, int> codeToIndex = new Dictionary<uint, int>();
            List<Vector3> vertexList = new List<Vector3>();
            foreach (DelaunayTriangle t in poly.Triangles)
            {
                foreach (var p in t.Points)
                {
                    if (codeToIndex.ContainsKey(p.VertexCode)) continue;
                    codeToIndex[p.VertexCode] = vertexList.Count;
                    Vector3 pos;
                    if (!codeToPosition.TryGetValue(p.VertexCode, out pos))
                    {
                        // This can happen in rare cases when we're hitting limits of floating-point precision.
                        // Rather than fail, let's just do the inverse rotation.
                        //					Output.PrintWarning("Vertex code lookup failed; using inverse rotation.");
                        if (!invRot.HasValue) invRot = Quaternion.Inverse(polygon.rotation);
                        pos = MultiplyQuaternionByVector(invRot.Value,new Vector3(p.Xf, p.Yf, z));
                    }
                    vertexList.Add(pos);
                }
            }

            List<int> indices = new List<int>();
            indices.Capacity = (poly.Triangles.Count * 3);
            foreach (DelaunayTriangle t in poly.Triangles)
            {
                indices.Add(codeToIndex[t.Points[0].VertexCode]);
                indices.Add(codeToIndex[t.Points[1].VertexCode]);
                indices.Add(codeToIndex[t.Points[2].VertexCode]);
            }

            if (thickness > 0)
            {
                AddRim(polygon.outside, thickness, vertexList, indices);
                foreach (var hole in polygon.holes)
                {
                    AddRim(hole, thickness, vertexList, indices);
                }
            }


            // Create the UV list, by looking up the closest point for each in our poly
            uvs = null;
            if (polygon.outsideUVs != null)
            {
                uvs = new Vector2[vertexList.Count];
                for (int i = 0; i < vertexList.Count; i++)
                {
                    uvs[i] = polygon.ClosestUV(vertexList[i]);
                }
            }

            vertices = vertexList.ToArray();
            normals = new Vector3[vertices.Length];
			for (int i = 0; i < normals.Length; i++)
			{
                normals[i] = polygon.planeNormal;
            }
            triangles = indices.ToArray();
        }

        private static void AddRim(List<Vector3> contour, float thickness, List<Vector3> vertexList, List<int> indices)
        {
            //Add a rim
            int rimVertCount = 0;

            //Add extra triangles for poly outside and insides
            for (int i = 0; i < contour.Count; i++)
            {
                Vector3 topLeft;
                Vector3 topRight;
                Vector3 bottomLeft;
                Vector3 bottomRight;

                topLeft = new Vector3(contour[i].X, contour[i].Y, contour[i].Z);
                bottomLeft = new Vector3(contour[i].X, contour[i].Y - thickness, contour[i].Z);
                if (i == contour.Count - 1)
                {
                    //Close loop by ending with first
                    topRight = new Vector3(contour[0].X, contour[i].Y, contour[0].Z);
                    bottomRight = new Vector3(contour[0].Y, contour[i].Y - thickness, contour[0].Z);
                }
                else
                {
                    topRight = new Vector3(contour[i + 1].X, contour[i].Y, contour[i + 1].Z);
                    bottomRight = new Vector3(contour[i + 1].Z, contour[i].Y - thickness, contour[i + 1].Z);
                }
                var startIndex = vertexList.Count;

                vertexList.Add(topLeft);
                vertexList.Add(topRight);
                vertexList.Add(bottomLeft);
                vertexList.Add(bottomRight);

                indices.Add(startIndex + 2);
                indices.Add(startIndex + 1);
                indices.Add(startIndex);

                indices.Add(startIndex + 3);
                indices.Add(startIndex + 1);
                indices.Add(startIndex + 2);

                rimVertCount += 6;
            }
            //innerloops
        }

        /// <summary>
        /// Create a Mesh containing just the FIRST triangle in the given Polygon.
        /// (This is a much easier task since we can skip triangulation.)
        /// </summary>
        /// <returns>The freshly minted mesh.</returns>
        /// <param name="polygon">Polygon you want to make a triangle of.</param>
        public static void CreateTriangle(Polygon polygon, out Vector3[] vertices, out Vector3[] normals, out int[] indices, out Vector2[] uv)
        {
            // Create the vertex array
            vertices = new Vector3[3];
            vertices[0] = polygon.outside[0];
            vertices[1] = polygon.outside[1];
            vertices[2] = polygon.outside[2];
            // Create the indices array
            indices = new int[3] { 0, 1, 2 };
            // Create the UV list, by looking up the closest point for each in our poly
            uv = null;
            if (polygon.outsideUVs != null)
            {
                uv = new Vector2[3];
                for (int i = 0; i < 3; i++)
                {
                    uv[i] = polygon.ClosestUV(vertices[i]);
                }
            }

            normals = new Vector3[vertices.Length];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = polygon.planeNormal;
            }
        }
    }
}