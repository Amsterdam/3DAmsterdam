using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConvertCoordinates;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.LayerSystem;
using Netherlands3D.Utilities;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public class Uitbouw : MonoBehaviour
    {
        private Vector2[] footprint;
        private MeshRenderer meshRenderer;
        private Mesh mesh;
        //private List<Vector2[]> perceel;

        [SerializeField]
        private BuildingMeshGenerator building;
        [SerializeField]
        private PerceelRenderer perceel;
        [SerializeField]
        float moveSpeed = 0.1f;

        public float Width { get; private set; }
        public float Depth { get; private set; }
        public float Height { get; private set; }

        private Vector3 extents;

        public Vector3 LeftCorner
        {
            get
            {
                return transform.position + transform.rotation * new Vector3(-extents.x, -extents.y, extents.z);
            }
        }

        public Vector3 RightCorner
        {
            get
            {
                return transform.position + transform.rotation * new Vector3(extents.x, -extents.y, extents.z);
            }
        }

        public Plane LeftCornerPlane
        {
            get
            {
                return new Plane(-transform.right, LeftCorner);
            }
        }

        public Plane RightCornerPlane
        {
            get
            {
                return new Plane(transform.right, RightCorner);
            }
        }

        public Plane SnapWall { get; private set; } = new Plane();

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            mesh = GetComponent<MeshFilter>().mesh;
        }

        private void Start()
        {
            SetDimensions(mesh.bounds.extents * 2);
        }

        private void SetDimensions(float w, float d, float h)
        {
            Width = w * transform.lossyScale.x;
            Depth = d * transform.lossyScale.z;
            Height = h * transform.lossyScale.y;

            extents = new Vector3(Width / 2, Height / 2, Depth / 2);
        }

        private void SetDimensions(Vector3 dim)
        {
            Width = dim.x * transform.lossyScale.x;
            Depth = dim.z * transform.lossyScale.z;
            Height = dim.y * transform.lossyScale.y;

            extents = new Vector3(Width / 2, Height / 2, Depth / 2);

            //print("Afmetingen uitbouw: breedte:" + Width + "\thoogte: " + Height + "\tdiepte:" + Depth);
        }

        private void Update()
        {
            if (perceel != null)
            {
                ProcessIfObjectIsInPerceelBounds();
            }

            ProcessUserInput();

            if (building)
            {
                SnapToBuilding(building);
            }
        }


        private void ProcessUserInput()
        {
            //temp
            if (Input.GetKey(KeyCode.Alpha1))
                transform.position -= transform.right * moveSpeed;

            if (Input.GetKey(KeyCode.Alpha2))
                transform.position += transform.right * moveSpeed;
        }

        private void ProcessIfObjectIsInPerceelBounds()
        {
            footprint = GenerateFootprint(mesh, transform.rotation, transform.lossyScale);

            if (PerceelBoundsRestriction.IsInPerceel(footprint, perceel.Perceel, transform.position))
            {
                meshRenderer.material.color = Color.green;
            }
            else
            {
                meshRenderer.material.color = Color.red;
            }
        }

        public Vector2[] GetFootprint()
        {
            return GenerateFootprint(mesh, transform.rotation, transform.lossyScale);
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


        private void SnapToBuilding(BuildingMeshGenerator building)
        {
            var uitbouwAttachDirection = transform.forward; //which side of the uitbouw is attatched to the house?
            LayerMask layerMask = LayerMask.GetMask("ActiveSelection"); //get the layer to test the boxcast to

            //test if there is a building in the uitbouw's path, snap to this surface if there is.
            //if (Physics.BoxCast(transform.position - uitbouwAttachDirection * 0.1f, extents, uitbouwAttachDirection, out RaycastHit hit, transform.rotation, Mathf.Infinity, layerMask)) // offset origin slightly to the back
            if (Physics.Raycast(transform.position, uitbouwAttachDirection, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                //print("Raycast hit");
                var dir = new Vector3(hit.normal.x, 0, hit.normal.z).normalized;
                uitbouwAttachDirection = -dir;
                transform.forward = -dir; //rotate towards correct direction
                transform.position = hit.point - uitbouwAttachDirection * Depth / 2;
                //transform.Translate(0, 0, hit.distance - 0.1f, Space.Self); //dont set position directly since the impact point can be different than the current x position. subtract 0.1f due to origin displacement          
            }
            //if there is no building in the uitbouw's path, raycast to the building center to find a new surface to snap to
            //todo: this is only guaranteed to work if the building center is inside the building's collider. it may fail if the building is strangely shaped and the collider center is outside of the collider
            //var layerToHit = LayerMask.NameToLayer("ActiveSelection"); //raycast uses the inverse for some reason
            else if (Physics.Raycast(transform.position + uitbouwAttachDirection * (Depth / 2), building.BuildingCenter - transform.position, out hit, Mathf.Infinity, layerMask))
            {
                print("Raycast failed, re-orienting to new wall");
                var dir = new Vector3(hit.normal.x, 0, hit.normal.z).normalized;
                uitbouwAttachDirection = -dir;
                transform.forward = -dir; //rotate towards correct direction
                transform.position = hit.point - uitbouwAttachDirection * Depth / 2;
            }

            SnapToGround(this.building);

            SnapWall = new Plane(hit.normal, hit.point);

            //Debug.DrawRay(transform.position + uitbouwAttachDirection * (depth / 2), building.BuildingCenter - transform.position, Color.magenta);
            if (Physics.Raycast(transform.position + uitbouwAttachDirection * (Depth / 2), building.BuildingCenter - transform.position, out hit, Mathf.Infinity, layerMask))
            {
                Debug.DrawLine(transform.position + uitbouwAttachDirection * (Depth / 2), hit.point, Color.cyan);
            }
        }

        void SnapToGround(BuildingMeshGenerator building)
        {
            transform.position = new Vector3(transform.position.x, building.GroundLevel + Height / 2, transform.position.z);
        }
    }
}