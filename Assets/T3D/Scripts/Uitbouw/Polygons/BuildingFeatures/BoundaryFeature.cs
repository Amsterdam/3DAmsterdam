using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class BoundaryFeatureSaveData : SaveDataContainer
    {
        //public int Id; //unique id for this boundary feature
        public string PrefabName; //which prefab should be instantiated?
        public WallSide WallSide;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Size;

        public BoundaryFeatureSaveData(string instanceId) : base(instanceId)
        {
        }

        public BoundaryFeatureSaveData(string instanceId, string prefabName, WallSide wallSide, Vector3 position, Quaternion rotation, Vector3 size) : base(instanceId)
        {
            PrefabName = prefabName;
            WallSide = wallSide;
            Position = position;
            Rotation = rotation;
            Size = size;
        }

        public void SetId(string id)
        {
            InstanceId = id;
        }

        public void UpdateDimensions(Vector3 position, Quaternion rotation, Vector3 size)
        {
            Position = position;
            Rotation = rotation;
            Size = size;
        }
    }

    public class BoundaryFeature : SquareSurface
    {
        public UitbouwMuur Wall { get; private set; }

        public EditMode ActiveMode { get; private set; }

        private DistanceMeasurement[] distanceMeasurements;

        private EditUI editUI;
        [SerializeField]
        private float editUIOffset = 0.2f;

        public string Id => SaveData.InstanceId; //set this when instantiating to differentiate it from other bfs in the save data
        public BoundaryFeatureSaveData SaveData { get; private set; }

        [SerializeField]
        private string displayName;
        public string DisplayName => displayName;

        protected override void Awake()
        {
            base.Awake();

            distanceMeasurements = GetComponents<DistanceMeasurement>();
            editUI = ServiceLocator.GetService<CoordinateNumbers>().CreateEditUI(this);
        }

        protected override void Start()
        {
            base.Start();
            SetMode(EditMode.None);
        }

        public void InitializeSaveData(string id, string prefabName)
        {
            if (SaveData == null)
            {
                SaveData = new BoundaryFeatureSaveData(id);
                SaveData.PrefabName = prefabName;
            }
            else
            {
                Debug.LogError("Save data for: " + id + " already exists as: " + SaveData.InstanceId);
            }
        }

        public void LoadData()
        {
            transform.rotation = SaveData.Rotation;
            transform.position = SaveData.Position;

            var shapeableUitbouw = RestrictionChecker.ActiveUitbouw as ShapeableUitbouw;
            if (shapeableUitbouw)
            {
                var side = SaveData.WallSide;
                var wall = shapeableUitbouw.GetWall(side);
                SetWall(wall);
            }

            SetSize(SaveData.Size);
        }

        public void SetWall(UitbouwMuur wall)
        {
            Surface.SolidSurfacePolygon.UpdateVertices(GetVertices());

            //remove the hole from the current wall, if the current wall exists
            if (Wall != null)
            {
                Surface.SetParent(null);
                Wall.Surface.TryRemoveHole(Surface.SolidSurfacePolygon);
            }
            //set the new wall
            Wall = wall;
            SaveData.WallSide = wall.Side;

            //add the hole to the new wall, if the new wall exists
            if (Wall != null)
            {
                ClipSizeToWallSize();
                Wall.Surface.TryAddHole(Surface.SolidSurfacePolygon); //add the hole to the new walli
                Surface.SetParent(wall.Surface);
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

        public override void SetSize(Vector2 size)
        {
            base.SetSize(size);
            if (Wall)
            {
                ClipSizeToWallSize();
            }
        }

        protected override void Update()
        {
            base.Update();
            SnapToWall();
            SetButtonPositions();
            LimitPositionOnWall();

            if (transform.parent != Wall.transform.parent)
            {
                transform.SetParent(Wall.transform.parent, true); //for some reason doing this in LoadData() breaks the positioning, so it is done here
            }

            SaveData.UpdateDimensions(transform.position, transform.rotation, Size);
        }

        private void SnapToWall()
        {
            transform.position = Wall.WallPlane.ClosestPointOnPlane(transform.position);
        }

        private void SetButtonPositions()
        {
            var trCorner = GetCorner(rightBound, topBound);
            var dir = (trCorner - transform.position).normalized;
            editUI.AlignWithWorldPosition(trCorner + dir * editUIOffset);
        }

        private void LimitPositionOnWall()
        {
            var maxOffsetX = Wall.Size.x / 2 - Size.x / 2;
            var projectedPositionX = Vector3.ProjectOnPlane(transform.position, Wall.transform.up);
            var projectedCenterX = Vector3.ProjectOnPlane(Wall.transform.position, Wall.transform.up);
            if (Vector3.Distance(projectedPositionX, projectedCenterX) > maxOffsetX) //distance too big, clipping needed
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
            var maxPos = direction * maxOffset - (direction * 0.03f); //hack 3cm margin to ensure the edge does not become to thin to cause problems for the CityJSON validation
            var minPos = -direction * maxOffset + (direction * 0.03f);

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
            editUI.UpdateSprites(newMode);
        }

        public void DeleteFeature()
        {
            Destroy(editUI.gameObject);

            Surface.SetParent(null);

            if (Wall)
                Wall.Surface.TryRemoveHole(Surface.SolidSurfacePolygon);

            var state = State.ActiveState as PlaceBoundaryFeaturesState;
            if (state)
            {
                state.RemoveBoundaryFeatureFromSaveData(this);
            }

            SaveData.DeleteSaveData();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (editUI)
                Destroy(editUI.gameObject);

            Surface.SetParent(null);

            if (Wall)
                Wall.Surface.TryRemoveHole(Surface.SolidSurfacePolygon);

            //remove the boundary feature from the list, extra measure to ensure list is reset when restarting the current session
            //replaced with a ne List<>() call in PlaceBoundaryFeatureState
            //if (PlaceBoundaryFeaturesState.SavedBoundaryFeatures.Contains(this))
            //{
            //    PlaceBoundaryFeaturesState.SavedBoundaryFeatures.Remove(this);
            //}
        }

        public void EditFeature()
        {
            if (ActiveMode == EditMode.Reposition)
                SetMode(EditMode.Resize);
            else
                SetMode(EditMode.Reposition);
        }
    }
}