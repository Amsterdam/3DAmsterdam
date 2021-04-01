#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ConvertCoordinates;
using System.IO;
using System.Threading;

namespace Netherlands3D.AssetGeneration.CityJSON
{
    public class ImportGeoJson : MonoBehaviour
    {
        [SerializeField]
        private Vector2 boundingBoxBottomLeft;
        [SerializeField]
        private Vector2 boundingBoxTopRight;

        [SerializeField]
        private int tileSize = 1000; //1x1 km

        [SerializeField]
        private double lodLevel = 2.2;

        public string objectType = "Buildings";
        public Material DefaultMaterial;

        [SerializeField]
        private string geoJsonSourceFilesFolder = "C:/Users/Sam/Desktop/downloaded_tiles_amsterdam/";

        [SerializeField]
        private string unityMeshAssetFolder = "BuildingAssets/";

        [SerializeField]
        [Tooltip("Leave 0 for all files")]
        private int maxFilesToProcess = 0;

        private Dictionary<Vector2, GameObject> generatedTiles;

        [SerializeField]
        private bool generateBuildingsAsSeperateObjects = true;
        [SerializeField]
        private bool renderInViewport = true;
        [SerializeField]
        private bool addBuildingsToFileNamedParents = false;

        [Header("Threading")]
        [SerializeField]
        private bool useThreading = false;
        [SerializeField]
        private int threads = 4;
        private List<Thread> runningThreads;

        [SerializeField]
        private Vector2 tileOffset;

        public void Start()
        {
            generatedTiles = new Dictionary<Vector2, GameObject>();
            ImportFilesFromFolder(geoJsonSourceFilesFolder, useThreading);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.X))
            {
                StopAllCoroutines();
                print("Aborted");
            }
        }

        private void BakeObjectsIntoTiles()
        {
            print("Baking objects into tiles");

            var xTiles = Mathf.RoundToInt(((float)boundingBoxTopRight.x - (float)boundingBoxBottomLeft.x) / (float)tileSize);
            var yTiles = Mathf.RoundToInt(((float)boundingBoxTopRight.y - (float)boundingBoxBottomLeft.y) / (float)tileSize);

            //Walk the tilegrid
            var tileRD = new Vector2Int(0,0);
            for (int x = 0; x < xTiles; x++)
			{
                tileRD.x = (int)boundingBoxBottomLeft.x + (x * tileSize);
                for (int y = 0; y < yTiles; y++)
                {
                    tileRD.y = (int)boundingBoxBottomLeft.y + (y * tileSize);

                    //Spawn our tile container
                    GameObject newTileContainer = new GameObject();
                    newTileContainer.transform.position = CoordConvert.RDtoUnity(tileRD + tileOffset);
                    newTileContainer.name = "tile_" + tileRD.x + "-" + tileRD.y;
                    generatedTiles.Add(tileRD, newTileContainer);

                    print("Created " + tileRD.x + "-" + tileRD.y);
                }
            }
        
            //Lets use meshrenderer bounds to get the buildings centre for now, and put them in the right tile
            MeshRenderer[] buildingMeshRenderers = GetComponentsInChildren<MeshRenderer>(true);
			foreach(MeshRenderer building in buildingMeshRenderers)
            {
                Vector3RD childRDCenter = CoordConvert.UnitytoRD(building.bounds.center);
                Vector2Int childGroupedTile = new Vector2Int(
                    Mathf.FloorToInt((float)childRDCenter.x / tileSize) * tileSize, 
                    Mathf.FloorToInt((float)childRDCenter.y / tileSize) * tileSize
                );
                print("Building tile " + childGroupedTile.x + "-" + childGroupedTile.y);

                //If we have a tile for this object, put it in.
                if (generatedTiles.TryGetValue(childGroupedTile, out GameObject targetParent))
                {
                    building.gameObject.transform.SetParent(targetParent.transform, true);
                }
            }

            //Bake the tiles
            foreach(GameObject tile in generatedTiles.Values)
            {
                CreateBuildingTile(tile, tile.gameObject.transform.position);
            }
		}

        private void CreateBuildingTile(GameObject buildingTile, Vector3 worldPosition)
        {
            buildingTile.transform.position = Vector3.zero;

            string assetName = unityMeshAssetFolder + buildingTile.name + ".asset";

            MeshFilter[] meshFilters = buildingTile.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            var totalVertexCount = 0;
            for (int i = 0; i < combine.Length; i++)
            {
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

                Mesh treeMesh = meshFilters[i].mesh;
                combine[i].mesh = treeMesh;
                meshFilters[i].gameObject.SetActive(false);
            }

            Mesh newCombinedMesh = new Mesh();
            if (totalVertexCount > 65536) //In case we go over the 16bit ( 2^16 ) index count, increase the indexformat.
                newCombinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            if (meshFilters.Length > 0)
            {
                newCombinedMesh.name = buildingTile.name;
                newCombinedMesh.CombineMeshes(combine, true);
            }

            buildingTile.AddComponent<MeshFilter>().sharedMesh = newCombinedMesh;
            buildingTile.AddComponent<MeshRenderer>().material = DefaultMaterial;
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(newCombinedMesh, assetName);
            AssetDatabase.SaveAssets();
#endif

            buildingTile.transform.position = worldPosition;
        }

        private void ImportFilesFromFolder(string folderName, bool threaded = false)
        {
            var info = new DirectoryInfo(folderName);
            var files = info.GetFiles();

            if (threaded)
            {
                runningThreads = new List<Thread>();
                for (int i = 0; i < threads; i++)
                {
                    Thread thread = new Thread(() => ParseFiles(files, i));
                    runningThreads.Add(thread);
                    thread.Start();
                }
            }
            else
            {
                StartCoroutine(ParseFilesWithFeedback(files));
            }
        }

        private IEnumerator ParseFilesWithFeedback(FileInfo[] fileInfo)
        {
            //First create gameobjects for all the buildigns we parse
            for (int i = 0; i < fileInfo.Length; i++)
            {
                if (i > maxFilesToProcess) continue;

                var file = fileInfo[i];
                Debug.Log("Parsing file nr. " + i + " / " + fileInfo.Length);

                CreateAsGameObjects(file.FullName, file.Name);
                yield return new WaitForEndOfFrame();
            }

            //Now bake the tiles with combined geometry
            BakeObjectsIntoTiles();
        }
        private GameObject CreateAsGameObjects(string filepath, string filename = "")
        {
            CityModel citymodel = new CityModel(filepath);
            List<Building> buildings = citymodel.LoadBuildings(2.2);

            var targetParent = this.gameObject;
            if (addBuildingsToFileNamedParents)
            {
                GameObject newContainer = new GameObject();
                newContainer.name = filename;
                targetParent = newContainer;
            }

            CreateGameObjects creator = new CreateGameObjects();
            creator.minimizeMeshes = !generateBuildingsAsSeperateObjects;
            creator.singleMeshBuildings = generateBuildingsAsSeperateObjects;
            creator.createPrefabs = false; //Do not auto create assets. We want to do this with our own method here
            creator.enableRenderers = renderInViewport;

            creator.CreateBuildings(buildings, new Vector3Double(), DefaultMaterial, targetParent);

            return targetParent;
        }

        private void ParseFiles(FileInfo[] fileInfo, int threadId = -1)
        {
            for (int i = 0; i < fileInfo.Length; i++)
            {
                if (threadId > -1 && i % threadId != 0) continue;

                var file = fileInfo[i];

                string[] fileNameParts = file.Name.Replace(".json", "").Split('_');
                /*
                var id = fileNameParts[0];
                var count = fileNameParts[1];

                var xmin = double.Parse(fileNameParts[3]);
                var ymin = double.Parse(fileNameParts[4]);
                var xmax = double.Parse(fileNameParts[5]);
                var ymax = double.Parse(fileNameParts[6]);
                */
                Debug.Log("Parsing file nr. " + i + " / " + fileInfo.Length);

                CreateAsGameObjects(file.FullName, file.Name);
            }
        }

        static void SavePrefab(GameObject container, string X, string Y, int LOD, string objectType)
        {
            MeshFilter[] mfs = container.GetComponentsInChildren<MeshFilter>();
            string objectFolder = CreateAssetFolder("Assets", objectType);
            string LODfolder = CreateAssetFolder(objectFolder, "LOD" + LOD);
            string SquareFolder = CreateAssetFolder(LODfolder, X + "_" + Y);
            string MeshFolder = CreateAssetFolder(SquareFolder, "meshes");
            //string PrefabFolder = CreateAssetFolder(SquareFolder, "Prefabs");
            string dataFolder = CreateAssetFolder(SquareFolder, "data");

            ObjectMappingClass objectMappingClass = ScriptableObject.CreateInstance<ObjectMappingClass>();
            objectMappingClass.ids = container.GetComponent<ObjectMapping>().BagID;
            objectMappingClass.triangleCount = container.GetComponent<ObjectMapping>().TriangleCount;
            Vector2[] meshUV = container.GetComponent<MeshFilter>().mesh.uv;
            objectMappingClass.vectorMap = container.GetComponent<ObjectMapping>().vectorIDs;
            List<Vector2> mappedUVs = new List<Vector2>();
            Vector2Int TextureSize = ObjectIDMapping.GetTextureSize(objectMappingClass.ids.Count);
            for (int i = 0; i < objectMappingClass.ids.Count; i++)
            {
                mappedUVs.Add(ObjectIDMapping.GetUV(i, TextureSize));
            }

            objectMappingClass.mappedUVs = mappedUVs;
            //objectMappingClass.TextureSize = TextureSize;
            objectMappingClass.uvs = meshUV;

            string typeName = objectType.ToLower();

            container.GetComponent<MeshFilter>().mesh.uv = null;
            AssetDatabase.CreateAsset(objectMappingClass, dataFolder + "/" + X + "_" + Y + "_" + typeName + "_lod" + LOD + "-data.asset");
            AssetDatabase.SaveAssets();
            AssetImporter.GetAtPath(dataFolder + "/" + X + "_" + Y + "_" + typeName + "_lod" + LOD + "-data.asset").SetAssetBundleNameAndVariant(X + "_" + Y + "_" + typeName + "_lod" + LOD + "-data", "");
            int meshcounter = 0;
            foreach (MeshFilter mf in mfs)
            {
                AssetDatabase.CreateAsset(mf.sharedMesh, MeshFolder + "/" + X + "_" + Y + "_" + typeName + "_lod" + LOD + ".mesh");
                AssetDatabase.SaveAssets();
                AssetImporter.GetAtPath(MeshFolder + "/" + X + "_" + Y + "_" + typeName + "_lod" + LOD + ".mesh").SetAssetBundleNameAndVariant(typeName + "_" + X + "_" + Y + "_lod" + LOD, "");
                meshcounter++;
            }
            AssetDatabase.SaveAssets();
            //PrefabUtility.SaveAsPrefabAssetAndConnect(container, PrefabFolder + "/" + container.name + ".prefab",InteractionMode.AutomatedAction);
            //AssetImporter.GetAtPath(PrefabFolder + "/" + container.name + ".prefab").SetAssetBundleNameAndVariant("Building_" + X + "_" +Y + "_LOD" + LOD, "");
        }

        static string CreateAssetFolder(string folderpath, string foldername)
        {
            if (!AssetDatabase.IsValidFolder(folderpath + "/" + foldername))
            {
                AssetDatabase.CreateFolder(folderpath, foldername);
            }
            return folderpath + "/" + foldername;
        }
    }
}
#endif