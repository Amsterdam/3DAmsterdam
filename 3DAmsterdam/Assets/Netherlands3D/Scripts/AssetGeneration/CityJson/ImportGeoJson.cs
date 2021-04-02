#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ConvertCoordinates;
using System.IO;
using System.Threading;
using Netherlands3D.LayerSystem;

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
        private string unityMeshAssetFolder = "Assets/3DAmsterdam/BuildingTileAssets/";

        [SerializeField]
        [Tooltip("Leave 0 for all files")]
        private int maxFilesToProcess = 0;

        private Dictionary<Vector2, GameObject> generatedTiles;

        [SerializeField]
        private bool renderInViewport = true;
        [SerializeField]
        private bool generateAssetFiles = false;
        [SerializeField]
        private bool allowEmptyTileGeneration = false;
        [SerializeField]
        private bool useFileNameBoundingBoxFiltering = true;

        [SerializeField]
        private Vector2 tileOffset;

        [Header("Threading")]
        [SerializeField]
        private bool useThreading = false;
        [SerializeField]
        private int threads = 4;
        private List<Thread> runningThreads;

        private CreateGameObjects creator;
        private List<Building> buildings;
        private CityModel cityModel;

        public void Start()
        {
            generatedTiles = new Dictionary<Vector2, GameObject>();
            StartCoroutine(CreateTilesAndReadInGeoJSON());
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.X))
            {
                StopAllCoroutines();
                print("Aborted");
            }
        }

        private IEnumerator CreateTilesAndReadInGeoJSON()
        {
            print("Baking objects into tiles");
            var xTiles = Mathf.RoundToInt(((float)boundingBoxTopRight.x - (float)boundingBoxBottomLeft.x) / (float)tileSize);
            var yTiles = Mathf.RoundToInt(((float)boundingBoxTopRight.y - (float)boundingBoxBottomLeft.y) / (float)tileSize);

            var totalTiles = xTiles * yTiles;
            int currentTile = 0;

            //Walk the tilegrid
            var tileRD = new Vector2Int(0,0);
            for (int x = 0; x < xTiles; x++)
			{
                tileRD.x = (int)boundingBoxBottomLeft.x + (x * tileSize);
                for (int y = 0; y < yTiles; y++)
                {
                    currentTile++;

                    tileRD.y = (int)boundingBoxBottomLeft.y + (y * tileSize);

                    //Spawn our tile container
                    GameObject newTileContainer = new GameObject();
                    newTileContainer.transform.position = CoordConvert.RDtoUnity(tileRD + tileOffset);
                    newTileContainer.name = "buildings_" + tileRD.x + "_" + tileRD.y + "." + lodLevel;

                    //Load GEOJsons that overlap this tile
                    ParseSpecificFiles(tileRD);
                    yield return new WaitForEndOfFrame();

                    //Now move them into the tile if their centerpoint is within our defined tile region
                    int buildingsAdded = MoveChildrenIntoTile(tileRD, newTileContainer, true);
                    yield return new WaitForEndOfFrame();

                    //Now bake the tile into an asset file
                    CreateBuildingTile(newTileContainer, newTileContainer.transform.position);
                    print("Created tile " + currentTile + "/" + totalTiles + " with " + buildingsAdded + " buildings -> " + newTileContainer.name);

                    yield return new WaitForEndOfFrame();
                }
                print("Done!");
            }
		}

        private int MoveChildrenIntoTile(Vector2Int rd, GameObject targetParentTile, bool removeOutside = false)
        {
            //Lets use meshrenderer bounds to get the buildings centre for now, and put them in the right tile
            Vector3RD childRDCenter;
            Vector2Int childGroupedTile;
            int buildingsAdded = 0;

            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
            for (int i = renderers.Length - 1; i >= 0; i--)
			{
                var building = renderers[i];
                childRDCenter = CoordConvert.UnitytoRD(building.bounds.center);
                childGroupedTile = new Vector2Int(
                    Mathf.FloorToInt((float)childRDCenter.x / tileSize) * tileSize,
                    Mathf.FloorToInt((float)childRDCenter.y / tileSize) * tileSize
                );

                //If we have a tile for this object, put it in.
                if (childGroupedTile == rd)
                {
                    buildingsAdded++;
                    building.transform.SetParent(targetParentTile.transform, true);
                }
                else if(removeOutside){
                    //This child is not in our tile. destroy it
                    Destroy(building.GetComponent<MeshFilter>().sharedMesh);
                    Destroy(building.gameObject);

                }
            }
            return buildingsAdded;
        }

        private void CreateBuildingTile(GameObject buildingTile, Vector3 worldPosition)
        {
            MeshFilter[] meshFilters = buildingTile.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            //Construct the seperate metadata containing the seperation of the buildings
            ObjectMappingClass buildingMetaData = ScriptableObject.CreateInstance<ObjectMappingClass>();
            buildingMetaData.ids = new List<string>();
            foreach (var meshFilter in meshFilters)
            {
                buildingMetaData.ids.Add(meshFilter.gameObject.name);
            }
            var textureSize = ObjectIDMapping.GetTextureSize(buildingMetaData.ids.Count);
            List<Vector2> allObjectUVs = new List<Vector2>();
            List<int> allVectorMapIndices = new List<int>();
            buildingMetaData.uvs = allObjectUVs.ToArray();

            //Generate the combined tile mesh
            buildingTile.transform.position = Vector3.zero;

            string assetFileName = unityMeshAssetFolder + buildingTile.name + ".asset";
            string assetMetaDataFileName = unityMeshAssetFolder + buildingTile.name + "-data.asset";

            var totalVertexCount = 0;
            for (int i = 0; i < combine.Length; i++)
            {
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                Mesh buildingMesh = meshFilters[i].sharedMesh;
                totalVertexCount += buildingMesh.vertexCount;
                //Create UVS
                var buildingUV = ObjectIDMapping.GetUV(i, textureSize);
                for (int v = 0; v < buildingMesh.vertexCount; v++)
                {
                    //UV count should match vert count
                    allObjectUVs.Add(buildingUV);
                    //Create vector map reference for vert
                    allVectorMapIndices.Add(i);
                }

                combine[i].mesh = buildingMesh;
                meshFilters[i].gameObject.SetActive(false);
            }
            //Now add all the combined uvs to our metadata
            buildingMetaData.uvs = allObjectUVs.ToArray();
            buildingMetaData.vectorMap = allVectorMapIndices;

            Mesh newCombinedMesh = new Mesh();
            if (totalVertexCount > 65536) //In case we go over the 16bit ( 2^16 ) index count, increase the indexformat.
                newCombinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            if (meshFilters.Length > 0)
            {
                newCombinedMesh.name = buildingTile.name;
                newCombinedMesh.CombineMeshes(combine, true);
                newCombinedMesh.RecalculateNormals();
                newCombinedMesh.Optimize();

                //And clean up memory
                for (int i = 0; i < combine.Length; i++)
                {
                    Destroy(meshFilters[i].sharedMesh);
                    Destroy(meshFilters[i].gameObject);
                }
            }
            if (renderInViewport)
            {

                buildingTile.AddComponent<MeshFilter>().sharedMesh = newCombinedMesh;
                buildingTile.AddComponent<MeshRenderer>().material = DefaultMaterial;
                buildingTile.transform.position = worldPosition;
            }
            else{
                Destroy(buildingTile);
			}

#if UNITY_EDITOR
            if (generateAssetFiles)
            {
                AssetDatabase.CreateAsset(newCombinedMesh, assetFileName);
                AssetDatabase.CreateAsset(buildingMetaData, assetMetaDataFileName);
                AssetDatabase.SaveAssets();
            }
#endif
        }
        /*
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
        */
        private void ParseSpecificFiles(Vector2Int rdCoordinates)
        {
            //Read files list 
            var info = new DirectoryInfo(geoJsonSourceFilesFolder);
            var fileInfo = info.GetFiles();

            //First create gameobjects for all the buildigns we parse
            int parsed = 0;
            for (int i = 0; i < fileInfo.Length; i++)
            {
                if (i > maxFilesToProcess && maxFilesToProcess != 0) continue;

                var file = fileInfo[i];

                string[] fileNameParts = file.Name.Replace(".json", "").Split('_');

                var id = fileNameParts[0];
                var count = fileNameParts[1];

                var xmin = double.Parse(fileNameParts[3]);
                var ymin = double.Parse(fileNameParts[4]);
                var xmax = double.Parse(fileNameParts[5]);
                var ymax = double.Parse(fileNameParts[6]);

                if (xmin > rdCoordinates.x+tileSize || xmax < rdCoordinates.x || ymin > rdCoordinates.y+tileSize || ymax < rdCoordinates.y)
                {
                    //Skip if these filename bounds are not within our selected rectangle
                    continue;
                }

                CreateAsGameObjects(file.FullName, file.Name);
                parsed++;
            }
            print("Parsed GeoJSONS to fill tile: " + parsed);
        }

        private IEnumerator ParseFilesWithFeedback(FileInfo[] fileInfo)
        {
            //First create gameobjects for all the buildigns we parse
            for (int i = 0; i < fileInfo.Length; i++)
            {
                if (i > maxFilesToProcess && maxFilesToProcess != 0) continue;

                var file = fileInfo[i];
                if(useFileNameBoundingBoxFiltering)
                {
                    string[] fileNameParts = file.Name.Replace(".json", "").Split('_');
                    
                    var id = fileNameParts[0];
                    var count = fileNameParts[1];

                    var xmin = double.Parse(fileNameParts[3]);
                    var ymin = double.Parse(fileNameParts[4]);
                    var xmax = double.Parse(fileNameParts[5]);
                    var ymax = double.Parse(fileNameParts[6]);

                    if(xmin > boundingBoxTopRight.x || xmax < boundingBoxBottomLeft.x || ymin > boundingBoxTopRight.y || ymax < boundingBoxBottomLeft .y)
                    {
                        //Skip if these filename bounds are not within our selected rectangle
                        print("Skipping " + xmin + "," + ymin + "," + xmax + "," + ymax);
                        continue;
                    }
                }

                Debug.Log("Parsing file nr. " + i + " / " + fileInfo.Length + ": " + file.Name);
                CreateAsGameObjects(file.FullName, file.Name);
                yield return new WaitForEndOfFrame();
            }

            //Now bake the tiles with combined geometry
            StartCoroutine(CreateTilesAndReadInGeoJSON());
        }
        private GameObject CreateAsGameObjects(string filepath, string filename = "")
        {
            var targetParent = this.gameObject;
            try
            {
                cityModel = new CityModel(filepath);
                buildings = cityModel.LoadBuildings(2.2);

                creator = new CreateGameObjects();
                creator.minimizeMeshes = false;
                creator.singleMeshBuildings = true;
                creator.createPrefabs = false; //Do not auto create assets. We want to do this with our own method here
                creator.enableRenderers = renderInViewport;
                creator.CreateBuildings(buildings, new Vector3Double(), DefaultMaterial, targetParent);

                creator = null;
                cityModel = null;
                buildings = null;
            }
            catch 
            {
                Debug.Log("Something went wrong in " + filepath);
			}
            return targetParent;
        }
        /*
        private void ParseFiles(FileInfo[] fileInfo, int threadId = -1)
        {
            for (int i = 0; i < fileInfo.Length; i++)
            {
                if (threadId > -1 && i % threadId != 0) continue;

                var file = fileInfo[i];

                Debug.Log("Parsing file nr. " + i + " / " + fileInfo.Length);

                CreateAsGameObjects(file.FullName, file.Name);
            }
        }*/
        /*
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
            Vector2[] meshUV = container.GetComponent<MeshFilter>().mesh.uv;
            objectMappingClass.vectorMap = container.GetComponent<ObjectMapping>().vectorIDs;
            List<Vector2> mappedUVs = new List<Vector2>();
            Vector2Int TextureSize = ObjectIDMapping.GetTextureSize(objectMappingClass.ids.Count);
            for (int i = 0; i < objectMappingClass.ids.Count; i++)
            {
                mappedUVs.Add(ObjectIDMapping.GetUV(i, TextureSize));
            }
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
        }*/
    }
}
#endif