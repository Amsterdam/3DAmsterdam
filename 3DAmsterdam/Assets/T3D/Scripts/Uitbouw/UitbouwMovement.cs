using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    [RequireComponent(typeof(UitbouwBase))]
    public class UitbouwMovement : MonoBehaviour
    {
        private UitbouwBase uitbouw;
        public bool AllowDrag { get; private set; } = true;

        [SerializeField]
        private GameObject dragableAxisPrefab;
        private DragableAxis[] userMovementAxes;

        private void Awake()
        {
            uitbouw = GetComponent<UitbouwBase>();
        }

        private void Start()
        {
            AllowDrag = AllowDrag && T3DInit.Instance.IsEditMode;

            InitializeUserMovementAxes();
            SetAllowMovement(AllowDrag);
        }

        public void InitializeUserMovementAxes()
        {
            var colliders = GetComponentsInChildren<Collider>();
            userMovementAxes = new DragableAxis[colliders.Length];
            for (int i = 0; i < colliders.Length; i++)
            {
                userMovementAxes[i] = colliders[i].gameObject.AddComponent<DragableAxis>();
                userMovementAxes[i].SetUitbouw(uitbouw);
            }

            var arrowOffsetY = transform.up * (uitbouw.Extents.y - 0.01f);

            //userMovementAxes[colliders.Length] = DragableAxis.CreateDragableAxis(dragableAxisPrefab, uitbouw.LeftCenter - arrowOffsetY, Quaternion.AngleAxis(90, Vector3.up) * dragableAxisPrefab.transform.rotation, uitbouw);
            //userMovementAxes[colliders.Length + 1] = DragableAxis.CreateDragableAxis(dragableAxisPrefab, uitbouw.RightCenter - arrowOffsetY, Quaternion.AngleAxis(-90, Vector3.up) * dragableAxisPrefab.transform.rotation, uitbouw);
        }

        //private void SetArrowPositions()
        //{
        //    var arrowOffsetY = transform.up * (uitbouw.Extents.y - 0.01f);
        //    userMovementAxes[userMovementAxes.Length - 2].transform.position = uitbouw.LeftCenter - arrowOffsetY;
        //    userMovementAxes[userMovementAxes.Length - 1].transform.position = uitbouw.RightCenter - arrowOffsetY;
        //}

        private void Update()
        {
            if (AllowDrag)
                ProcessUserInput();
            LimitPositionOnWall();

            //SetArrowPositions();
        }

        private void ProcessUserInput()
        {
            foreach (var axis in userMovementAxes) //drag input
            {
                transform.position += axis.DeltaPosition;
            }
        }

        private void LimitPositionOnWall()
        {
            var extents = uitbouw.ActiveBuilding.SelectedWall.WallMesh.bounds.extents;
            var projectedExtents = Vector3.ProjectOnPlane(extents, Vector3.up);

            var projectedPosition = Vector3.ProjectOnPlane(transform.position, Vector3.up);
            var projectedCenter = Vector3.ProjectOnPlane(uitbouw.ActiveBuilding.SelectedWall.CenterPoint, Vector3.up);

            var maxOffset = projectedExtents.magnitude + uitbouw.Width / 2 - 0.1f; //subtract 0.1f to deal with rounding

            if (Vector3.Distance(projectedPosition, projectedCenter) > maxOffset)
            {
                ClipDistance(maxOffset);
            }
        }

        private void ClipDistance(float maxOffset)
        {
            var maxPos = uitbouw.ActiveBuilding.SelectedWall.CenterPoint + transform.right * maxOffset;
            var minPos = uitbouw.ActiveBuilding.SelectedWall.CenterPoint - transform.right * maxOffset;

            if (Vector3.Distance(transform.position, minPos) > Vector3.Distance(transform.position, maxPos))
                transform.position = maxPos;
            else
                transform.position = minPos;

            uitbouw.SnapToGround(uitbouw.ActiveBuilding);
        }

        public virtual void SetAllowMovement(bool allowed)
        {
            AllowDrag = allowed && T3DInit.Instance.IsEditMode;
            var measuring = GetComponent<UitbouwMeasurement>();
            measuring.DrawDistanceActive = allowed;

            //userMovementAxes[userMovementAxes.Length - 2].gameObject.SetActive(allowed);
            //userMovementAxes[userMovementAxes.Length - 1].gameObject.SetActive(allowed);
        }
    }
}