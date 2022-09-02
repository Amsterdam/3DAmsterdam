using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using T3D.LoadData;

namespace Netherlands3D.T3D.Uitbouw
{
    public class BuildingMeshGenerator : MonoBehaviour
    {
        public Vector3 BuildingCenter { get; private set; }
        public float GroundLevel { get; private set; }
        public float HeightLevel { get; private set; }
        public bool IsMonument { get; private set; }
        public bool IsBeschermd { get; private set; }
        public float Area { get; private set; }

        public WallSelector SelectedWall { get; private set; }

        //public Vector3[] RelativeBuildingCorners { get; private set; }
        public Vector3[] AbsoluteBuildingCorners { get; private set; }
        public Plane[] RoofEdgePlanes { get; private set; }
        private float roofEdgeYTolerance = 0.1f;

        public bool BuildingDataIsProcessed { get; private set; } = false;

        public delegate void BuildingDataProcessedEventHandler(BuildingMeshGenerator building);
        public event BuildingDataProcessedEventHandler BuildingDataProcessed;

        private void Awake()
        {
            SelectedWall = GetComponentInChildren<WallSelector>();
        }

        protected void Start()//in start to avoid race conditions
        {
            //base.Start();
            //ServiceLocator.GetService<MetadataLoader>().BuildingMetaDataLoaded += Instance_BuildingMetaDataLoaded;
            ServiceLocator.GetService<MetadataLoader>().BuildingOutlineLoaded += Instance_BuildingOutlineLoaded;
            //ServiceLocator.GetService<MetadataLoader>().CityJsonBagLoaded += OnCityJsonBagLoaded;
            ServiceLocator.GetService<MetadataLoader>().CityJsonBagReceived += BuildingMeshGenerator_CityJsonBagReceived;

            SessionSaver.Loader.LoadingCompleted += Loader_LoadingCompleted;
        }

        private void BuildingMeshGenerator_CityJsonBagReceived(string cityJson)
        {
            StartCoroutine(ParseBuildingCityJson(cityJson));
        }

        private IEnumerator ParseBuildingCityJson(string cityJson)
        {
            yield return new WaitUntil(() => RestrictionChecker.ActivePerceel.IsLoaded); //needed because perceelRadius is needed

            var cityJsonModel = new CityJsonModel(cityJson, new Vector3RD(), true);
            var meshes = CityJsonVisualiser.ParseCityJson(cityJsonModel, transform.localToWorldMatrix, true, true);
            var attributes = CityJsonVisualiser.GetAttributes(cityJsonModel.cityjsonNode["CityObjects"]);
            CityJsonVisualiser.AddExtensionNodes(cityJsonModel.cityjsonNode);
            //var combinedMesh = CityJsonVisualiser.CombineMeshes(meshes.Values.ToList(), transform.localToWorldMatrix);

            var cityObject = GetComponent<CityJSONToCityObject>();
            cityObject.SetNodes(meshes, attributes, cityJsonModel.vertices);
            var activeMesh = cityObject.SetMeshActive(2); //todo: not hardcode the active lod

            if (activeMesh)
                ProcessMesh(activeMesh);
        }

        private void ProcessMesh(Mesh mesh)
        {

            var col = gameObject.AddComponent<MeshCollider>();
            BuildingCenter = col.bounds.center;
            GroundLevel = BuildingCenter.y - col.bounds.extents.y; //hack: if the building geometry goes through the ground this will not work properly
            HeightLevel = BuildingCenter.y + col.bounds.extents.y;

            RoofEdgePlanes = ProcessRoofEdges(mesh);

            BuildingDataProcessed.Invoke(this); // it cannot be assumed if the perceel or building data loads + processes first due to the server requests, so this event is called to make sure the processed building information can be used by other classes
            BuildingDataIsProcessed = true;
        }

        private void Loader_LoadingCompleted(bool loadSucceeded)
        {
            IsMonument = ServiceLocator.GetService<T3DInit>().HTMLData.IsMonument;
            IsBeschermd = ServiceLocator.GetService<T3DInit>().HTMLData.IsBeschermd;
        }

        private Plane[] ProcessRoofEdges(Mesh buildingMesh)
        {
            var verts = buildingMesh.vertices;
            List<Plane> planes = new List<Plane>();
            List<float> yValues = new List<float>();

            foreach (var vert in verts)
            {
                var y = vert.y;
                if (!IsWithinAnyYToleranceRange(yValues, y, roofEdgeYTolerance))
                {
                    yValues.Add(y);
                    planes.Add(new Plane(-Vector3.up, y));
                }
            }

            return planes.ToArray();
        }

        private static bool IsWithinAnyYToleranceRange(List<float> yValues, float y, float tolerance)
        {
            var index = yValues.FindIndex(num => Mathf.Abs(num - y) < tolerance);
            return index != -1;
        }

        private void Instance_BuildingOutlineLoaded(object source, BuildingOutlineEventArgs args)
        {
            Area = args.TotalArea;
            StartCoroutine(ProcessCorners(args.Outline));
        }

        private IEnumerator ProcessCorners(List<Vector2[]> coords)
        {
            yield return new WaitUntil(() => BuildingDataIsProcessed); //wait until ground level is set

            var q = from i in coords
                    from p in i
                    select CoordConvert.RDtoUnity(p) into v3
                    select new Vector3(v3.x, GroundLevel, v3.z);

            AbsoluteBuildingCorners = q.ToArray();
        }

    }
}
