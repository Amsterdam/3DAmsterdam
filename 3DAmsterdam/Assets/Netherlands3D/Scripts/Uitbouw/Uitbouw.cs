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
        private BuildingMeshGenerator building;

        public float Width { get; private set; }
        public float Depth { get; private set; }
        public float Height { get; private set; }
        public float Area { get { return Width * Depth; } }

        private Vector3 extents;

        [SerializeField]
        private GameObject dragableAxisPrefab;
        private DragableAxis[] userMovementAxes;

        //[Header("Walls")]
        private UitbouwMuur left => GetWall(WallSide.Left);
        private UitbouwMuur right => GetWall(WallSide.Right);
        private UitbouwMuur bottom => GetWall(WallSide.Bottom);
        private UitbouwMuur top => GetWall(WallSide.Top);
        private UitbouwMuur front => GetWall(WallSide.Front);
        private UitbouwMuur back => GetWall(WallSide.Back);

        [SerializeField]
        private UitbouwMuur[] walls;

        public Vector3 LeftCorner
        {
            get
            {
                return left.transform.position + transform.rotation * new Vector3(0, -extents.y, extents.z);
            }
        }

        public Vector3 RightCorner
        {
            get
            {
                return right.transform.position + transform.rotation * new Vector3(0, -extents.y, extents.z);
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

        public Plane GroundPlane
        {
            get
            {
                return new Plane(Vector3.up, building.GroundLevel);
            }
        }

        public Plane SnapWall { get; private set; } = new Plane();


        public Vector3 CenterAttatchPoint
        {
            get
            {
                return back.transform.position;
            }
        }

        public Vector3 CenterPoint
        {
            get
            {
                return front.transform.position + (back.transform.position - front.transform.position) / 2;
            }
        }
        //public Vector3 CenterPosition { get; private set; }

        public static Uitbouw Instance;

        private void Awake()
        {
            Instance = this;
            //meshRenderer = GetComponent<MeshRenderer>();
            //mesh = GetComponent<MeshFilter>().mesh;
            //SetDimensions(mesh.bounds.extents * 2);
            //userMovementAxis = GetComponentInChildren<DragableAxis>();
            building = RestrictionChecker.ActiveBuilding;
            UpdateDimensions();
        }

        private void Start()
        {
            userMovementAxes = new DragableAxis[2 + walls.Length];

            for (int i = 0; i < walls.Length; i++)
            {
                userMovementAxes[i] = walls[i].gameObject.AddComponent<DragableAxis>();
                userMovementAxes[i].SetUitbouw(this);
            }

            //var arrowOffsetX = transform.right * extents.x;
            var arrowOffsetY = transform.up * (extents.y - 0.01f);

            userMovementAxes[walls.Length] = DragableAxis.CreateDragableAxis(dragableAxisPrefab, left.transform.position - arrowOffsetY, Quaternion.AngleAxis(90, Vector3.up) * dragableAxisPrefab.transform.rotation, this);
            userMovementAxes[walls.Length + 1] = DragableAxis.CreateDragableAxis(dragableAxisPrefab, right.transform.position - arrowOffsetY, Quaternion.AngleAxis(-90, Vector3.up) * dragableAxisPrefab.transform.rotation, this);
        }

        public void UpdateDimensions()
        {
            SetDimensions(left, right, bottom, top, front, back);
        }

        private void SetDimensions(UitbouwMuur left, UitbouwMuur right, UitbouwMuur bottom, UitbouwMuur top, UitbouwMuur front, UitbouwMuur back)
        {
            //var xComponent = right.transform.localPosition - left.transform.localPosition;
            //var yComponent = top.transform.localPosition - bottom.transform.localPosition;
            //var zComponent = back.transform.localPosition - front.transform.localPosition;

            //var localOrigin = new Vector3(xComponent.x / 2, yComponent.y / 2, zComponent.z / 2);

            //var widthVector = right.transform.position - left.transform.position;
            var widthVector = Vector3.Project(right.transform.position - left.transform.position, left.transform.forward);
            var heightVector = Vector3.Project(top.transform.position - bottom.transform.position, bottom.transform.forward);
            var depthVector = Vector3.Project(back.transform.position - front.transform.position, front.transform.forward);

            SetDimensions(widthVector.magnitude, depthVector.magnitude, heightVector.magnitude);
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
            UpdateDimensions();
            SetArrowPositions();

            //if (perceel != null)
            //{
            //    ProcessIfObjectIsInPerceelBounds();
            //}

            ProcessUserInput();
            
            //SnapToBuilding(building);
            
        }

        private void SetArrowPositions()
        {
            var arrowOffsetY = transform.up * (extents.y - 0.01f);
            userMovementAxes[walls.Length].transform.position = left.transform.position - arrowOffsetY;
            userMovementAxes[walls.Length + 1].transform.position = right.transform.position - arrowOffsetY;
        }

        private void ProcessUserInput()
        {
            foreach (var axis in userMovementAxes) //drag input
            {
                transform.position += axis.DeltaPosition;
            }

            if (building)
            {
                if (building.SelectedWall.WallIsSelected)
                {
                    SnapToWall(building.SelectedWall);
                    SnapToGround(building);
                }
                else
                { 
                    //SnapToBuilding(building);
                }
            }
        }

        private void SnapToWall(WallSelector selectedWall)
        {
            var dir = selectedWall.WallPlane.normal;
            transform.forward = -dir; //rotate towards correct direction

            //remove local x component
            var diff = selectedWall.WallPlane.ClosestPointOnPlane(transform.position) - transform.position; //moveVector
            var rotatedPoint = Quaternion.Inverse(transform.rotation) * diff; //moveVector aligned in world space
            rotatedPoint.x = 0; //remove horizontal component
            var projectedPoint = transform.rotation * rotatedPoint; //rotate back
            var newPoint = projectedPoint + transform.position; // apply movevector

            transform.position = newPoint;//hit.point - uitbouwAttachDirection * Depth / 2;
        }

        //private void ProcessIfObjectIsInPerceelBounds()
        //{
        //    footprint = GetFootprint();// GenerateFootprint(meshes, transform.rotation, transform.lossyScale);

        //    if (PerceelBoundsRestriction.IsInPerceel(footprint, perceel.Perceel, transform.position))
        //    {
        //        meshRenderer.material.color = Color.green;
        //    }
        //    else
        //    {
        //        meshRenderer.material.color = Color.red;
        //    }
        //}

        public Vector2[] GetFootprint()
        {
            var meshFilters = new MeshFilter[] {
                left.MeshFilter,
                right.MeshFilter,
                top.MeshFilter,
                bottom.MeshFilter,
                front.MeshFilter,
                back.MeshFilter
            };

            return GenerateFootprint(meshFilters);//, transform.rotation, transform.lossyScale);
        }

        public static Vector2[] GenerateFootprint(MeshFilter[] meshFilters)//Mesh[] meshes, Quaternion rotation, Vector3 scale)
        {
            var footprint = new List<Vector2>();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                var verts = meshFilters[i].mesh.vertices;

                for (int j = 0; j < verts.Length; j++)
                {
                    var transformedVert = verts[j];
                    transformedVert.Scale(meshFilters[i].transform.lossyScale);
                    transformedVert = meshFilters[i].transform.rotation * transformedVert;
                    //var vert = Vector3.ProjectOnPlane(rotatedVert, Vector3.up);
                    var vert = new Vector2(transformedVert.x, transformedVert.z);

                    footprint.Add(vert); //todo: optimize so that only the outline is in the footprint
                }
            }
            return footprint.ToArray();
        }

        private void SnapToBuilding(BuildingMeshGenerator building)
        {
            var uitbouwAttachDirection = transform.forward; //which side of the uitbouw is attatched to the house?
            LayerMask layerMask = LayerMask.GetMask("ActiveSelection"); //get the layer to test the boxcast to

            //test if there is a building in the uitbouw's path, snap to this surface if there is.
            //if (Physics.BoxCast(transform.position - uitbouwAttachDirection * 0.1f, extents, uitbouwAttachDirection, out RaycastHit hit, transform.rotation, Mathf.Infinity, layerMask)) // offset origin slightly to the back
            if (Physics.Raycast(CenterPoint, uitbouwAttachDirection, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                //print("Raycast hit");
                var dir = new Vector3(hit.normal.x, 0, hit.normal.z).normalized;
                transform.forward = -dir; //rotate towards correct direction

                //remove local x component
                var diff = hit.point - transform.position; //moveVector
                var rotatedPoint = Quaternion.Inverse(transform.rotation) * diff; //moveVector aligned in world space
                rotatedPoint.x = 0; //remove horizontal component
                var projectedPoint = transform.rotation * rotatedPoint; //rotate back
                var newPoint = projectedPoint + transform.position; // apply movevector

                transform.position = newPoint;//hit.point - uitbouwAttachDirection * Depth / 2;
                //transform.Translate(0, 0, hit.distance - 0.1f, Space.Self); //dont set position directly since the impact point can be different than the current x position. subtract 0.1f due to origin displacement          
            }
            //if there is no building in the uitbouw's path, raycast to the building center to find a new surface to snap to
            //todo: this is only guaranteed to work if the building center is inside the building's collider. it may fail if the building is strangely shaped and the collider center is outside of the collider
            //var layerToHit = LayerMask.NameToLayer("ActiveSelection"); //raycast uses the inverse for some reason
            else if (Physics.Raycast(CenterAttatchPoint, building.BuildingCenter - CenterAttatchPoint, out hit, Mathf.Infinity, layerMask))
            {
                print("Raycast failed, re-orienting to new wall");
                var dir = new Vector3(hit.normal.x, 0, hit.normal.z).normalized;
                transform.forward = -dir; //rotate towards correct direction

                //remove local x component
                var diff = hit.point - transform.position;
                var rotatedPoint = Quaternion.Inverse(transform.rotation) * diff;
                rotatedPoint.x = 0;
                var projectedPoint = transform.rotation * rotatedPoint;
                var newPoint = projectedPoint + transform.position;

                transform.position = newPoint; //hit.point - uitbouwAttachDirection * Depth / 2;

                //recalculate mouse offset position, since the uitbouw (and its controls) changed orientation
                foreach (var axis in userMovementAxes)
                {
                    axis.RecalculateOffset();
                }
            }

            SnapToGround(this.building);

            SnapWall = new Plane(hit.normal, hit.point);

            Debug.DrawRay(CenterAttatchPoint, building.BuildingCenter - transform.position, Color.magenta);
            if (Physics.Raycast(CenterAttatchPoint, building.BuildingCenter - transform.position, out hit, Mathf.Infinity, layerMask))
            {
                Debug.DrawLine(CenterPoint, hit.point, Color.cyan);
            }
        }

        void SnapToGround(BuildingMeshGenerator building)
        {
            transform.position = new Vector3(transform.position.x, building.GroundLevel /*+ Height / 2*/, transform.position.z);
        }

        public void MoveWall(WallSide side, float delta)
        {
            //safety deactivation
            foreach (var wall in walls)
            {
                wall.SetActive(false);
            }

            UitbouwMuur activeWall = GetWall(side);
            activeWall.MoveWall(delta);
        }

        private UitbouwMuur GetWall(WallSide side)
        {
            var muur = walls.FirstOrDefault(x => x.Side == side);
            return muur;
            //return walls.FirstOrDefault(x => x.Side == side);
        }
    }
}
