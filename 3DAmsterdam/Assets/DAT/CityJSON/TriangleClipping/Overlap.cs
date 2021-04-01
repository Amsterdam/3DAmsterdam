using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TriangleClipping
{
    public class Plane
    {
        public Vector3 pos;

        public Vector3 normal;

        public Plane(Vector3 pos, Vector3 normal)
        {
            this.pos = pos;

            this.normal = normal;
        }
    }

    public static class Intersections
    {
        //Get the coordinate if we know a ray-plane is intersecting
        public static Vector3 GetRayPlaneIntersectionCoordinate(Vector3 planePos, Vector3 planeNormal, Vector3 rayStart, Vector3 rayDir)
        {
            float denominator = Vector3.Dot(-planeNormal, rayDir);

            Vector3 vecBetween = planePos - rayStart;

            float t = Vector3.Dot(vecBetween, -planeNormal) / denominator;

            Vector3 intersectionPoint = rayStart + rayDir * t;

            return intersectionPoint;
        }
    }

    public static class Geometry
    {
        //Is a point to the left, to the right, or on a plane
        //https://gamedevelopment.tutsplus.com/tutorials/understanding-sutherland-hodgman-clipping-for-physics-engines--gamedev-11917
        //Notice that the plane normal doesnt have to be normalized
        public static float DistanceFromPointToPlane(Vector3 planeNormal, Vector3 planePos, Vector3 pointPos)
        {
            //Positive distance denotes that the point p is on the front side of the plane 
            //Negative means it's on the back side
            float distance = Vector3.Dot(planeNormal, pointPos - planePos);

            return distance;
        }
    }
    public static class MathUtility
    {
        //Clamp list indices
        //Will even work if index is larger/smaller than listSize, so can loop multiple times
        public static int ClampListIndex(int index, int listSize)
        {
            index = ((index % listSize) + listSize) % listSize;

            return index;
        }
    }


    public static class SutherlandHodgman
    {
        //Assumes the polygons are oriented counter clockwise
        //poly_1 is the polygon we want to cut
        //Assumes the polygon we want to remove from the other polygon is convex, so poly_2 has to be convex
        //We will end up with the intersection of the polygons
        public static List<Vector3> ClipPolygon(List<Vector3> poly_1, List<Vector3> poly_2)
        {
            //Calculate the clipping planes
            List<Plane> clippingPlanes = new List<Plane>();

            for (int i = 0; i < poly_2.Count; i++)
            {
                int iPlusOne = MathUtility.ClampListIndex(i + 1, poly_2.Count);

                Vector3 v1 = poly_2[i];
                Vector3 v2 = poly_2[iPlusOne];

                //Doesnt have to be center but easier to debug
                Vector3 planePos = (v1 + v2) * 0.5f;

                Vector3 planeDir = v2 - v1;

                //Should point inwards
                Vector3 planeNormal = new Vector3(-planeDir.z, 0f, planeDir.x).normalized;

                //Gizmos.DrawRay(planePos, planeNormal * 0.1f);

                clippingPlanes.Add(new Plane(planePos, planeNormal));
            }



            List<Vector3> vertices = ClipPolygon(poly_1, clippingPlanes);

            return vertices;
        }



        //Sometimes its more efficient to calculate the planes once before we call the method
        //if we want to cut several polygons with the same planes
        public static List<Vector3> ClipPolygon(List<Vector3> poly_1, List<Plane> clippingPlanes)
        {
            //Clone the vertices because we will remove vertices from this list
            List<Vector3> vertices = new List<Vector3>(poly_1);

            //Save the new vertices temporarily in this list before transfering them to vertices
            List<Vector3> vertices_tmp = new List<Vector3>();


            //Clip the polygon
            for (int i = 0; i < clippingPlanes.Count; i++)
            {
                Plane plane = clippingPlanes[i];

                for (int j = 0; j < vertices.Count; j++)
                {
                    int jPlusOne = MathUtility.ClampListIndex(j + 1, vertices.Count);

                    Vector3 v1 = vertices[j];
                    Vector3 v2 = vertices[jPlusOne];

                    //Calculate the distance to the plane from each vertex
                    //This is how we will know if they are inside or outside
                    //If they are inside, the distance is positive, which is why the planes normals have to be oriented to the inside
                    float dist_to_v1 = Geometry.DistanceFromPointToPlane(plane.normal, plane.pos, v1);
                    float dist_to_v2 = Geometry.DistanceFromPointToPlane(plane.normal, plane.pos, v2);

                    //Case 1. Both are outside (= to the right), do nothing                    

                    //Case 2. Both are inside (= to the left), save v2
                    if (dist_to_v1 >= 0f && dist_to_v2 >= 0f)
                    {
                        vertices_tmp.Add(v2);
                    }
                    //Case 3. Outside -> Inside, save intersection point and v2
                    else if (dist_to_v1 <= 0f && dist_to_v2 >= 0f)
                    {
                        Vector3 rayDir = (v2 - v1).normalized;

                        Vector3 intersectionPoint = Intersections.GetRayPlaneIntersectionCoordinate(plane.pos, plane.normal, v1, rayDir);

                        vertices_tmp.Add(intersectionPoint);

                        vertices_tmp.Add(v2);
                    }
                    //Case 4. Inside -> Outside, save intersection point
                    else if (dist_to_v1 >= 0f && dist_to_v2 <= 0f)
                    {
                        Vector3 rayDir = (v2 - v1).normalized;

                        Vector3 intersectionPoint = Intersections.GetRayPlaneIntersectionCoordinate(plane.pos, plane.normal, v1, rayDir);

                        vertices_tmp.Add(intersectionPoint);
                    }
                }

                //Add the new vertices to the list of vertices
                vertices.Clear();

                vertices.AddRange(vertices_tmp);

                vertices_tmp.Clear();
            }

            return vertices;
        }
    }
}