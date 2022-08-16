using ConvertCoordinates;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Netherlands3D.Utilities
{
    public struct CenterAndRadius
    {
        public Vector2 Center;
        public float Radius;
    }

    public static class GeometryCalculator
    {
        public static bool ContainsPoint(Vector2[] polyPoints, Vector2 p)
        {
            var j = polyPoints.Length - 1;
            var inside = false;
            for (int i = 0; i < polyPoints.Length; j = i++)
            {
                var pi = polyPoints[i];
                var pj = polyPoints[j];
                if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                    (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                    inside = !inside;
            }
            return inside;
        }

        //adapted from: http://wiki.unity3d.com/index.php?title=Triangulator
        public static int[] Triangulate(Vector2[] points)
        {
            List<int> indices = new List<int>();

            int n = points.Length;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area(points) > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(points, u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        public static float Area(Vector2[] points)
        {
            int n = points.Length;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = points[p];
                Vector2 qval = points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }

        private static bool Snip(Vector2[] points, int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = points[V[u]];
            Vector2 B = points[V[v]];
            Vector2 C = points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        public static bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }

        public static CenterAndRadius GetCenterAndRadius(List<Vector2[]> allpoints)
        {
            var points = allpoints.SelectMany(o => o).ToArray();
            return GetCenterAndRadius(points);


        }

        public static CenterAndRadius GetCenterAndRadius(Vector2[] points)
        {
            var minx = points.Min(v => v.x);
            var maxx = points.Max(v => v.x);
            var miny = points.Min(v => v.y);
            var maxy = points.Max(v => v.y);
            var centerx = minx + ((maxx - minx) / 2);
            var centery = miny + ((maxy - miny) / 2);

            var center = new Vector2(centerx, centery);

            var centerAndRadius = new CenterAndRadius()
            {
                Center = center,
                Radius = Vector2.Distance(center, new Vector2(maxx, maxy))
            };

            return centerAndRadius;            
        }

    }
}