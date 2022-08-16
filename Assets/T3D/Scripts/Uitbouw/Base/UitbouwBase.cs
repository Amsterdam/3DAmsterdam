using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Interface;
using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public class UitbouwBaseSaveDataContainer : SaveDataContainer
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }

    public abstract class UitbouwBase : MonoBehaviour
    {
        //public CityObject CityObject { get; private set; }

        protected BuildingMeshGenerator building;
        public BuildingMeshGenerator ActiveBuilding => building;

        public float Width { get; private set; }
        public float Depth { get; private set; }
        public float Height { get; private set; }
        public float Area { get { return Width * Depth; } }

        public Vector3 Extents { get; private set; }

        public abstract Vector3 LeftCenter { get; }
        public abstract Vector3 RightCenter { get; }
        public abstract Vector3 TopCenter { get; }
        public abstract Vector3 BottomCenter { get; }
        public abstract Vector3 FrontCenter { get; }
        public abstract Vector3 BackCenter { get; }

        private UitbouwBaseSaveDataContainer saveData;

        private DragableAxis[] userMovementAxes;
        public DragableAxis[] UserMovementAxes => userMovementAxes;

        [SerializeField]
        private UitbouwTransformGizmo gizmoPrefab;
        public UitbouwTransformGizmo Gizmo { get; private set; }

        public Vector3 LeftCorner
        {
            get
            {
                return LeftCenter + transform.rotation * new Vector3(0, -Extents.y, Extents.z);
            }
        }

        public Vector3 RightCorner
        {
            get
            {
                return RightCenter + transform.rotation * new Vector3(0, -Extents.y, Extents.z);
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

        public Vector3 CenterAttatchPoint
        {
            get
            {
                return BackCenter;
            }
        }

        public Vector3 CenterPoint
        {
            get
            {
                return FrontCenter + (BackCenter - FrontCenter) / 2;
            }
        }

        public bool IsDraggingMovementAxis
        {
            get
            {
                foreach (var axis in userMovementAxes)
                {
                    if (axis.IsDragging)
                        return true;
                }
                return false;
            }
        }

        protected virtual void Awake()
        {
            saveData = new UitbouwBaseSaveDataContainer();
            InitializeUserMovementAxes();
        }

        public abstract void UpdateDimensions();

        public void InitializeUserMovementAxes()
        {
            var colliders = GetComponentsInChildren<Collider>();
            userMovementAxes = new DragableAxis[colliders.Length + 1];
            for (int i = 0; i < colliders.Length; i++)
            {
                var axis = colliders[i].GetComponent<DragableAxis>();
                if (axis)
                    userMovementAxes[i] = axis;
                else
                    userMovementAxes[i] = colliders[i].gameObject.AddComponent<DragableAxis>();
                userMovementAxes[i].SetUitbouw(this);
            }

            if (!Gizmo)
            {
                var gizmo = DragableAxis.CreateDragableAxis(gizmoPrefab.gameObject, BottomCenter, gizmoPrefab.transform.rotation, this);
                //userMovementAxes[userMovementAxes.Length - 1] = gizmo;
                Gizmo = gizmo as UitbouwTransformGizmo;
            }

            userMovementAxes[userMovementAxes.Length - 1] = Gizmo;

            //var arrowOffsetY = transform.up * (uitbouw.Extents.y - 0.01f);

            //userMovementAxes[colliders.Length] = DragableAxis.CreateDragableAxis(dragableAxisPrefab, uitbouw.LeftCenter - arrowOffsetY, Quaternion.AngleAxis(90, Vector3.up) * dragableAxisPrefab.transform.rotation, uitbouw);
            //userMovementAxes[colliders.Length + 1] = DragableAxis.CreateDragableAxis(dragableAxisPrefab, uitbouw.RightCenter - arrowOffsetY, Quaternion.AngleAxis(-90, Vector3.up) * dragableAxisPrefab.transform.rotation, uitbouw);
        }

        protected virtual void Start()
        {
            building = RestrictionChecker.ActiveBuilding; //in start to ensure ActiveBuilding is set
            if (SessionSaver.LoadPreviousSession)
            {
                transform.SetPositionAndRotation(saveData.Position, saveData.Rotation);
            }
        }

        protected virtual void Update()
        {
            UpdateGizmo();
            saveData.Position = transform.position;
            saveData.Rotation = transform.rotation;
        }

        public void EnableGizmo(bool enable)
        {
            Gizmo.SetActive(enable, true, !ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall, !ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall, true);
        }

        private void UpdateGizmo()
        {
            Gizmo.transform.position = BottomCenter + 0.001f * Vector3.one; //avoid z-fighting
            Gizmo.SetDiameter(Extents.magnitude * 2);
        }

        protected void SetDimensions(float w, float d, float h)
        {
            Width = w * transform.lossyScale.x;
            Depth = d * transform.lossyScale.z;
            Height = h * transform.lossyScale.y;

            Extents = new Vector3(Width / 2, Height / 2, Depth / 2);
        }

        protected void SetDimensions(Vector3 size)
        {
            SetDimensions(size.x, size.z, size.y);
        }

        public Vector2[] GetFootprint()
        {
            var meshFilters = GetComponentsInChildren<MeshFilter>();
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
    }
}
