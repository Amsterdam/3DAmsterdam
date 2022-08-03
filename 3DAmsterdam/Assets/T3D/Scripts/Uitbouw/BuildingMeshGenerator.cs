using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.LayerSystem;
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
            var cityJsonModel = new CityJsonModel(cityJson, new Vector3RD(), true);
            var meshes = CityJsonVisualiser.ParseCityJson(cityJsonModel, transform.localToWorldMatrix);
            var combinedMesh = CityJsonVisualiser.CombineMeshes(meshes.Values.ToList(), transform.localToWorldMatrix);

            foreach (var pair in meshes)
            {
                var mesh = pair.Value;
                if (mesh != null)
                {
                    var cityObject = GetComponent<CityJSONToCityObject>();
                    cityObject.SetNode(pair.Key, cityJsonModel.vertices);
                    //uitbouw.AddCityObject(newCityObject);
                    //AddMeshGameObject(key, mesh);
                }
            }
            ProcessMesh(combinedMesh);
        }

        private void ProcessMesh(Mesh mesh)
        {

            //transform.position = args.TileOffset;
            var mf = GetComponent<MeshFilter>();
            mf.mesh = mesh;

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

        //private void Instance_BuildingMetaDataLoaded(object source, ObjectDataEventArgs args)
        //{
        //    var buildingMesh = ExtractBuildingMesh(args.ObjectData, args.ObjectData.highlightIDs[0]);
        //    transform.position = args.TileOffset;
        //    var mf = GetComponent<MeshFilter>();
        //    mf.mesh = buildingMesh;

        //    var col = gameObject.AddComponent<MeshCollider>();
        //    BuildingCenter = col.bounds.center;
        //    GroundLevel = BuildingCenter.y - col.bounds.extents.y; //hack: if the building geometry goes through the ground this will not work properly
        //    HeightLevel = BuildingCenter.y + col.bounds.extents.y;

        //    RoofEdgePlanes = ProcessRoofEdges(buildingMesh);

        //    BuildingDataProcessed.Invoke(this); // it cannot be assumed if the perceel or building data loads + processes first due to the server requests, so this event is called to make sure the processed building information can be used by other classes
        //    BuildingDataIsProcessed = true;
        //}

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

        public static Mesh ExtractBuildingMesh(ObjectData objectData, string id)
        {
            var idIndex = objectData.ids.FindIndex(o => o.Contains(id));

            //copy mesh data to avoid getting a copy every iteration in the loop
            var sourceVerts = objectData.mesh.vertices;
            var sourceTriangles = objectData.mesh.triangles;
            var sourceUVs = objectData.uvs;

            List<int> vertIndices = new List<int>();
            for (int i = 0; i < objectData.vectorMap.Count; i++)
            {
                //var vertcount = objectData

                if (objectData.vectorMap[i] == idIndex)
                {
                    vertIndices.Add(i);
                }
            }

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();

            List<int> usedVerts = new List<int>();
            for (int i = 0; i < sourceTriangles.Length; i += 3)
            {
                //check if the current triangle is part of the extracted verts
                if (vertIndices.Contains(sourceTriangles[i]))// || vertIndices.Contains(sourceTriangles[i + 1]) || vertIndices.Contains(sourceTriangles[i + 2]))
                {
                    //add matching triangle to my mesh
                    for (int j = 0; j < 3; j++)
                    {
                        //check if this vertex is already used
                        var existingVertIndex = usedVerts.FindIndex(x => x == sourceTriangles[i + j]);
                        int newTriIndex = -1;
                        if (existingVertIndex == -1) //vert not found, add this vert
                        {
                            vertices.Add(sourceVerts[sourceTriangles[i + j]]);
                            // uvs.Add(sourceUVs[sourceTriangles[i + j]]); 
                            newTriIndex = vertices.Count - 1;
                            usedVerts.Add(sourceTriangles[i + j]);
                        }
                        else //vert already in use, so just add the triangle index
                        {
                            newTriIndex = existingVertIndex;
                        }

                        triangles.Add(newTriIndex);
                    }
                }
            }

            //generate mesh from data
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            //  mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
