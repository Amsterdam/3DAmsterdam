using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class BoundaryFeatureSaveData
    {
        public static string BaseKey { get { return typeof(BoundaryFeatureSaveData).ToString(); } }

        //public string BaseKey;
        public SaveableInt Id; //unique id for this boundary feature
        public SaveableString PrefabName; //which prefab should be instantiated?
        public SaveableInt WallSide;
        public SaveableVector3 Position;
        public SaveableQuaternion Rotation;
        public SaveableVector3 Size;

        public BoundaryFeatureSaveData(int objectId)
        {
            var baseKey = BaseKey + "." + objectId;
            Id = new SaveableInt(baseKey + ".Id");
            PrefabName = new SaveableString(baseKey + ".PrefabName"); ;
            WallSide = new SaveableInt(baseKey + ".WallSide"); ;
            Position = new SaveableVector3(baseKey + ".Position");
            Rotation = new SaveableQuaternion(baseKey + ".Rotation");
            Size = new SaveableVector3(baseKey + ".Size");
        }

        public void UpdateData(int id, string prefabName, WallSide wallSide, Vector3 position, Quaternion rotation, Vector3 size)
        {
            Id.SetValue(id);
            PrefabName.SetValue(prefabName);
            WallSide.SetValue((int)wallSide);
            Position.SetValue(position);
            Rotation.SetValue(rotation);
            Size.SetValue(size);
        }

        public void UpdateWall(WallSide newWall)
        {
            WallSide.SetValue((int)newWall);
        }

        public void UpdateDimensions(Vector3 position, Quaternion rotation, Vector3 size)
        {
            Position.SetValue(position);
            Rotation.SetValue(rotation);
            Size.SetValue(size);
        }

        public void DeleteKeys()
        {
            Id.Delete();
            PrefabName.Delete();
            WallSide.Delete();
            Position.Delete();
            Rotation.Delete();
            Size.Delete();
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

        //private Vector3 deltaPos;

        //private string idKey;
        //private SaveableInt id;
        //private int instanceId;
        public int Id { get; private set; } //set this when instantiating to differentiate it from other bfs in the save data
        public string PrefabName { get; set; } //set this when instantiating to save the prefab to load the next time
        public BoundaryFeatureSaveData SaveData { get; private set; }

        [SerializeField]
        private string displayName;
        public string DisplayName => displayName;

        protected override void Awake()
        {
            base.Awake();

            //idKey = GetType().Namespace + GetType().ToString() + "." + GetInstanceID().ToString();
            //id = new SaveableInt(idKey);

            //featureTransform = transform.parent;
            distanceMeasurements = GetComponents<DistanceMeasurement>();
            editUI = CoordinateNumbers.Instance.CreateEditUI(this);
        }

        protected override void Start()
        {
            base.Start();
            SetMode(EditMode.None);
        }

        public void InitializeSaveData(int id)
        {
            if (SaveData == null)
            {
                Id = id;
                SaveData = new BoundaryFeatureSaveData(id);
            }
        }

        public void UpdateMetadata(int id, string prefabName)
        {
            Id = id;
            PrefabName = prefabName;

            var wallSide = (WallSide)SaveData.WallSide.Value;
            SaveData = new BoundaryFeatureSaveData(Id); // reset the key used to save data
            SaveData.UpdateData(Id, PrefabName, wallSide, transform.position, transform.rotation, Size);
        }

        public void LoadData(int id)
        {
            InitializeSaveData(id);

            PrefabName = SaveData.PrefabName.Value;
            var side = (WallSide)SaveData.WallSide.Value;

            transform.rotation = SaveData.Rotation.Value;
            transform.position = SaveData.Position.Value;

            var shapeableUitbouw = RestrictionChecker.ActiveUitbouw as ShapeableUitbouw;
            if (shapeableUitbouw)
            {
                var wall = shapeableUitbouw.GetWall(side);
                SetWall(wall);
            }

            SetSize(SaveData.Size.Value);

            //transform.SetParent(wall.transform.parent, true); //breaks the positioning for some reason
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
            SaveData.UpdateWall(wall.Side);

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
            //var pos = meshTransform.position + meshTransform.rotation * meshTransform.GetComponent<SpriteRenderer>().bounds.extents;
            var trCorner = GetCorner(rightBound, topBound);
            var dir = (trCorner - transform.position).normalized;
            editUI.AlignWithWorldPosition(trCorner + dir * editUIOffset);
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
            var maxPos = direction * maxOffset - (direction * 0.0001f);
            var minPos = -direction * maxOffset + (direction * 0.0001f);

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
            if (Wall)
                Wall.Surface.TryRemoveHole(Surface.SolidSurfacePolygon);

            var state = State.ActiveState as PlaceBoundaryFeaturesState;
            if (state)
            {
                state.RemoveBoundaryFeatureFromSaveData(this);
            }

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (editUI)
                Destroy(editUI.gameObject);
            //Destroy(gameObject);
            if (Wall)
                Wall.Surface.TryRemoveHole(Surface.SolidSurfacePolygon);

            //remove the boundary feature from the list, extra measure to ensure list is reset when restarting the current session
            if (PlaceBoundaryFeaturesState.SavedBoundaryFeatures.Contains(this))
            {
                PlaceBoundaryFeaturesState.SavedBoundaryFeatures.Remove(this);
            }
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