using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class BoundaryFeature : SquareSurface
    {
        public UitbouwMuur Wall { get; private set; }

        public EditMode ActiveMode { get; private set; }

        private DistanceMeasurement[] distanceMeasurements;

        private EditUI editUI;
        [SerializeField]
        private float editUIOffset = 0.2f;

        private Vector3 deltaPos;

        protected override void Awake()
        {
            base.Awake();
            //featureTransform = transform.parent;
            distanceMeasurements = GetComponents<DistanceMeasurement>();
            editUI = CoordinateNumbers.Instance.CreateEditUI(this);

            SetMode(EditMode.None);
        }

        public void SetWall(UitbouwMuur wall)
        {
            Surface.SolidSurfacePolygon.UpdateVertices(GetVertices());

            //remove the hole from the current wall, if the current wall exists
            if (Wall != null)
            {
                //Surface = Wall.GetComponent<CitySurface>();
                Wall.Surface.TryRemoveHole(Surface.SolidSurfacePolygon);
            }
            //set the new wall
            Wall = wall;

            //add the hole to the new wall, if the new wall exists
            if (Wall != null)
            {
                ClipSizeToWallSize();
                Wall.Surface.TryAddHole(Surface.SolidSurfacePolygon); //add the hole to the new wall
            }
        }

        private void ClipSizeToWallSize()
        {
            if (Size.x > Wall.Size.x)
            {
                SetSize(new Vector2(Wall.Size.x - 0.001f, Size.y));
            }
            if (Size.y > Wall.Size.y)
            {
                SetSize(new Vector2(Size.x, Wall.Size.y - 0.0001f));
            }
            //SetSize(Size - new Vector2(0.0001f, 0.0001f)); //ugly hack to remove 0.1mm, but it's to ensure the CityJson hole polygon does not glitch out when the height is exactly on the boundary line
        }

        protected override void Update()
        {
            base.Update();
            SnapToWall();
            SetButtonPositions();
            ProcessDrag();
            LimitPositionOnWall();
        }

        private void SnapToWall()
        {
            transform.position = Wall.WallPlane.ClosestPointOnPlane(transform.position);
        }

        private void SetButtonPositions()
        {
            //var pos = meshTransform.position + meshTransform.rotation * meshTransform.GetComponent<SpriteRenderer>().bounds.extents;
            var trCorner = GetCorner(rightBound, topBound);
            var dir = (trCorner - transform.position).normalized;
            editUI.AlignWithWorldPosition(trCorner + dir * editUIOffset);
        }

        private void ProcessDrag()
        {
            Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
            var mask = LayerMask.GetMask("Maskable");
            bool casted = Physics.Raycast(ray, out var hit, Mathf.Infinity, mask);

            if (casted && Input.GetMouseButtonDown(0))
            {
                deltaPos = hit.point - transform.position;
            }

            ObjectClickHandler.GetDrag(out var wallCollider, mask);
            if (ObjectClickHandler.GetDragOnObject(GetComponentInChildren<Collider>(), true) && casted && Wall.GetComponent<Collider>() == wallCollider)
            {
                transform.position = hit.point - deltaPos;
            }
        }

        private void LimitPositionOnWall()
        {
            var maxOffsetX = Wall.Size.x / 2 - Size.x / 2;
            var projectedPositionX = Vector3.ProjectOnPlane(transform.position, Wall.transform.up);
            var projectedCenterX = Vector3.ProjectOnPlane(Wall.transform.position, Wall.transform.up);
            if (Vector3.Distance(projectedPositionX, projectedCenterX) > maxOffsetX)
            {
                var xComp = ClipDistance(transform.right, maxOffsetX);
                var yComp = Vector3.ProjectOnPlane(transform.position - Wall.transform.position, transform.right);
                transform.position = Wall.transform.position + xComp + yComp;
            }

            var maxOffsetY = Wall.Size.y / 2 - Size.y / 2;
            var projectedPositionY = Vector3.ProjectOnPlane(transform.position, Wall.transform.right);
            var projectedCenterY = Vector3.ProjectOnPlane(Wall.transform.position, Wall.transform.right);
            if (Vector3.Distance(projectedPositionY, projectedCenterY) > maxOffsetY)
            {
                var xComp = Vector3.ProjectOnPlane(transform.position - Wall.transform.position, transform.up);
                var yComp = ClipDistance(transform.up, maxOffsetY);
                transform.position = Wall.transform.position + xComp + yComp;
            }
        }

        private Vector3 ClipDistance(Vector3 direction, float maxOffset)
        {
            var maxPos = direction * maxOffset;
            var minPos = -direction * maxOffset;

            if (Vector3.Distance(transform.position, Wall.transform.position + minPos) > Vector3.Distance(transform.position, Wall.transform.position + maxPos))
                return maxPos;
            else
                return minPos;
        }

        public void SetMode(EditMode newMode)
        {
            ActiveMode = newMode;
            //var distanceMeasurements = this.distanceMeasurements;
            for (int i = 0; i < distanceMeasurements.Length; i++)
            {
                distanceMeasurements[i].DrawDistanceActive = distanceMeasurements[i].Mode == newMode;
            }

            editUI.gameObject.SetActive(newMode != EditMode.None);
        }

        public void DeleteFeature()
        {
            Destroy(editUI.gameObject);
            if (Wall)
                Wall.Surface.TryRemoveHole(Surface.SolidSurfacePolygon);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (editUI)
                Destroy(editUI.gameObject);
            //Destroy(gameObject);
            if (Wall)
                Wall.Surface.TryRemoveHole(Surface.SolidSurfacePolygon);
        }

        public void EditFeature()
        {
            if (ActiveMode == EditMode.Reposition)
                SetMode(EditMode.Resize);
            else
                SetMode(EditMode.Reposition);

            editUI.UpdateSprites(ActiveMode);
        }
    }
}