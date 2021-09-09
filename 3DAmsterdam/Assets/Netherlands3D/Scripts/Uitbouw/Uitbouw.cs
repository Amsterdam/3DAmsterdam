using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConvertCoordinates;
using UnityEngine;

namespace Netherlands3D.T3D.Perceel
{
    public class Uitbouw : MonoBehaviour
    {
        private Vector2[] footprint;
        private MeshRenderer meshRenderer;
        private Mesh mesh;
        private List<Vector2[]> perceel;

        private Vector3 oldPosition;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            mesh = GetComponent<MeshFilter>().mesh;
        }

        private void Start()
        {
            perceel = PerceelRenderer.Instance.Perceel;
            PerceelRenderer.Instance.PerceelDataLoaded += Instance_PerceelDataLoaded;
        }

        private void Instance_PerceelDataLoaded(object source, PerceelDataEventArgs args)
        {
            perceel = args.Perceel;
        }

        private void Update()
        {
            if (perceel != null)
            {
                CheckIfObjectIsInPerceelBounds();
            }
        }

        private void CheckIfObjectIsInPerceelBounds()
        {
            footprint = GenerateFootprint(mesh, transform.rotation, transform.lossyScale);

            if (IsInPerceel(footprint, perceel, transform.position))
            {
                meshRenderer.material.color = Color.green;
                print("In bounds");
                //oldPosition = transform.position;
            }
            else
            {
                meshRenderer.material.color = Color.red;
                print("out bounds");
                //transform.position = oldPosition;
            }
        }

        public static Vector2[] GenerateFootprint(Mesh mesh, Quaternion rotation, Vector3 scale)
        {
            var verts = mesh.vertices;
            var footprint = new List<Vector2>();

            for (int i = 0; i < verts.Length; i++)
            {
                var transformedVert = verts[i];
                transformedVert.Scale(scale);
                transformedVert = rotation * transformedVert;
                //var vert = Vector3.ProjectOnPlane(rotatedVert, Vector3.up);
                var vert = new Vector2(transformedVert.x, transformedVert.z);

                footprint.Add(vert); //todo: optimize so that only the outline is in the footprint
            }
            return footprint.ToArray();
        }

        public static bool IsInPerceel(Vector2[] footprint, List<Vector2[]> perceel, Vector3 positionOffset)
        {
            var q = from i in perceel
                    from p in i
                    select CoordConvert.RDtoUnity(p) into v3
                    select new Vector2(v3.x, v3.z);

            var polyPoints = q.ToArray(); //todo: test for non-contiguous  perceels

            foreach (var vert in footprint)
            {
                if (!PerceelRenderer.ContainsPoint(polyPoints, vert + new Vector2(positionOffset.x, positionOffset.z)))
                {
                    return false;
                }
            }
            return true;
        }

        //public static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        //{
        //    return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        //}

        //public static bool IsPointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        //{
        //    float d1, d2, d3;
        //    bool has_neg, has_pos;

        //    d1 = Sign(pt, v1, v2);
        //    d2 = Sign(pt, v2, v3);
        //    d3 = Sign(pt, v3, v1);

        //    has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        //    has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        //    return !(has_neg && has_pos);
        //}

        //private void OnDrawGizmos()
        //{
        //    footprint = GenerateFootprint(GetComponent<MeshFilter>().mesh, transform.rotation);

        //    foreach (var vert in footprint)
        //    {
        //        Gizmos.DrawSphere(new Vector3(transform.position.x + vert.x, 0, transform.position.z + vert.y), 0.1f);
        //        print(vert);
        //    }

        //    for (int i = 0; i < footprint.Length - 1; i++)
        //    {
        //        Debug.DrawLine(transform.position + new Vector3(footprint[i].x, 0, footprint[i].y), transform.position + new Vector3(footprint[i + 1].x, 0, footprint[i + 1].y));
        //    }
        //    Debug.DrawLine(transform.position + new Vector3(footprint[0].x, 0, footprint[0].y), transform.position + new Vector3(footprint[footprint.Length - 1].x, 0, footprint[footprint.Length - 1].y));
        //}
    }
}