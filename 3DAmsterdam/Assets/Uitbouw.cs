using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Test
{
    public class Uitbouw : MonoBehaviour
    {
        Vector2[] footprint;
        public static List<Vector2[]> perceel;
        public static Vector3 tileOffset;

        private void Start()
        {
        }

        private void Update()
        {
            footprint = GenerateFootprint(GetComponent<MeshFilter>().mesh, transform.rotation, transform.position);

            if (perceel == null)
                return;

            if (IsInPerceel(footprint, perceel, transform.position))
            {
                GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else
            {
                GetComponent<MeshRenderer>().material.color = Color.green;
            }

            foreach (var vert in footprint)
            {
                Debug.DrawLine(transform.position, new Vector3(transform.position.x + vert.x, 2, transform.position.z + vert.y), Color.red);
            }
        }

        public static Vector2[] GenerateFootprint(Mesh mesh, Quaternion rotation, Vector3 positionOffset)
        {
            var verts = mesh.vertices;
            var footprint = new List<Vector2>();
            //print(verts.Length);
            for (int i = 0; i < verts.Length; i++)
            {
                //var vertx = verts[i].x;
                //var verty = verts[i].z;
                //var vert = new Vector2(vertx, verty);
                var rotatedVert = rotation * verts[i];
                //var vert = Vector3.ProjectOnPlane(rotatedVert, Vector3.up);
                var vert = new Vector2(rotatedVert.x, rotatedVert.z);
                if (i < 2 || !PerceelRenderer.ContainsPoint(footprint.ToArray(), vert))
                {
                    footprint.Add(vert);
                }
                if (i > 0)
                    Debug.DrawLine(positionOffset + new Vector3(footprint[i].x, 0, footprint[i].y), positionOffset + new Vector3(footprint[i - 1].x, 0, footprint[i - 1].y), Color.cyan);
            }
            return footprint.ToArray();
        }

        public static bool IsInPerceel(Vector2[] footprint, List<Vector2[]> perceel, Vector3 positionOffset)
        {
            foreach (var vert in footprint)
            {
                foreach (Vector2[] perceelPart in perceel)
                {
                    if (!PerceelRenderer.ContainsPoint(perceelPart, vert + new Vector2(positionOffset.x, positionOffset.z)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void OnDrawGizmos()
        {
            footprint = GenerateFootprint(GetComponent<MeshFilter>().mesh, transform.rotation, transform.position);

            foreach (var vert in footprint)
            {
                Gizmos.DrawSphere(new Vector3(transform.position.x + vert.x, 0, transform.position.z + vert.y), 0.1f);
                print(vert);
            }

            for (int i = 0; i < footprint.Length - 1; i++)
            {
                Debug.DrawLine(transform.position + new Vector3(footprint[i].x, 0, footprint[i].y), transform.position + new Vector3(footprint[i + 1].x, 0, footprint[i + 1].y));
            }
            Debug.DrawLine(transform.position + new Vector3(footprint[0].x, 0, footprint[0].y), transform.position + new Vector3(footprint[footprint.Length - 1].x, 0, footprint[footprint.Length - 1].y));
        }
    }
}