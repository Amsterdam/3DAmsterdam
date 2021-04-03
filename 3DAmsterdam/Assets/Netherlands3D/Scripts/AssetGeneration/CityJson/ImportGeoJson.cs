#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ConvertCoordinates;
using System.IO;
using System.Threading;
using Netherlands3D.LayerSystem;
using System.Linq;
using SimpleJSON;
using UnityEngine.UI;
using UnityEngine.Networking;

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

        [SerializeField]
        private int lodSlot = 2;

        private List<Vector3> allVerts;
        private List<int> meshTriangles;

        private int vertIndex;

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
        private bool skipExistingFiles = true;
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

        private string previewBackdropImage = "https://geodata.nationaalgeoregister.nl/ahn2/wms?service=WMS&request=GetMap&layers=ahn2_5m&BBOX=109000,474000,141000,501000&WIDTH={w}&HEIGHT={h}&VERSION=1&wmtver=1.1&styles=&format=image/png&srs=EPSG:28992";
        private Texture2D drawIntoPixels;
        private RawImage rawImage;

        [SerializeField]
        private GameObject optionalObjectToEnableWhenFinished;

        public void Start()
        {
            rawImage = FindObjectOfType<RawImage>();
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

            //Show a previewmap
            drawIntoPixels = new Texture2D(yTiles, yTiles, TextureFormat.RGBA32, false);
            var downloadUrl = previewBackdropImage.Replace("{w}", xTiles.ToString()).Replace("{h}", yTiles.ToString());
            print(downloadUrl);
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(downloadUrl);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                drawIntoPixels = ((DownloadHandlerTexture)www.downloadHandler).texture;
            }

            drawIntoPixels.filterMode = FilterMode.Point;
            rawImage.texture = drawIntoPixels;

            //Walk the tilegrid
            var tileRD = new Vector2Int(0,0);
            for (int x = 0; x < xTiles; x++)
			{
                tileRD.x = (int)boundingBoxBottomLeft.x + (x * tileSize);
                for (int y = 0; y < yTiles; y++)
                {
                    currentTile++;

                    tileRD.y = (int)boundingBoxBottomLeft.y + (y * tileSize);

                    string tileName = "buildings_" + tileRD.x + "_" + tileRD.y + "." + lodLevel;
                    
                    //Maybe skip files?
                    string assetFileName = unityMeshAssetFolder + tileName + ".asset";
                    if (skipExistingFiles && File.Exists(Application.dataPath + "/../" + assetFileName)) 
                    {
                        print("Skipping existing tile: " + Application.dataPath + "/../" + assetFileName);
                        drawIntoPixels.SetPixel(x, y, Color.grey);
                        drawIntoPixels.Apply();
                        yield return new WaitForEndOfFrame();
                        continue;
                    }

                    //Spawn our tile container
                    GameObject newTileContainer = new GameObject();
                    newTileContainer.transform.position = CoordConvert.RDtoUnity(tileRD + tileOffset);
                    newTileContainer.name = tileName;

                    print("Parsing JSON files for tile " + currentTile + " / " + totalTiles + ": " + newTileContainer.name);
                    yield return new WaitForEndOfFrame();

                    //Load GEOJsons that overlap this tile
                    yield return StartCoroutine(ParseSpecificFiles(tileRD));
                    
                    yield return new WaitForEndOfFrame();

                    //Now move them into the tile if their centerpoint is within our defined tile region
                    int buildingsAdded = MoveChildrenIntoTile(tileRD, newTileContainer, true);
                    if (buildingsAdded == 0)
                    {
                        drawIntoPixels.SetPixel(x, y, Color.black);
                    }
                    else
                    {
                        drawIntoPixels.SetPixel(x, y, Color.Lerp(Color.black, Color.green, (float)buildingsAdded / (float)tileSize));
                    }
                    drawIntoPixels.Apply();

                    yield return new WaitForEndOfFrame();

                    //Now bake the tile into an asset file
                    CreateBuildingTile(newTileContainer, newTileContainer.transform.position);
                    print("Created tile " + currentTile + "/" + totalTiles + " with " + buildingsAdded + " buildings -> " + newTileContainer.name);

                    yield return new WaitForEndOfFrame();
                }
            }
            print("Done!");
            if (optionalObjectToEnableWhenFinished)
                optionalObjectToEnableWhenFinished.SetActive(true);

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

        private IEnumerator ParseSpecificFiles(Vector2Int rdCoordinates)
        {
            //Read files list 
            var info = new DirectoryInfo(geoJsonSourceFilesFolder);
            var fileInfo = info.GetFiles();

            //First create gameobjects for all the buildigns we parse
            int parsed = 0;
            for (int i = 0; i < fileInfo.Length; i++)
            {
                var file = fileInfo[i];

                string[] fileNameParts = file.Name.Replace(".json", "").Split('_');

                var id = fileNameParts[0];
                var count = fileNameParts[1];

                var xmin = double.Parse(fileNameParts[3]);
                var ymin = double.Parse(fileNameParts[4]);
                var xmax = double.Parse(fileNameParts[5]);
                var ymax = double.Parse(fileNameParts[6]);

                if (xmin > rdCoordinates.x + tileSize || xmax < rdCoordinates.x || ymin > rdCoordinates.y + tileSize || ymax < rdCoordinates.y)
                {
                    //Skip if these filename bounds are not within our selected rectangle
                    continue;
                }
                Debug.Log("Parsing " + file.Name);
                yield return new WaitForEndOfFrame();

                var jsonstring = File.ReadAllText(file.FullName);
                var cityjsonNode = JSON.Parse(jsonstring);

                if (cityjsonNode["CityObjects"] == null)
                {
                    Debug.Log("FAILURE PARSING: " + file.Name);
                    continue; //Failed to parse the json
                }
                //get vertices
                allVerts = new List<Vector3>();

                //optionaly parse transform scale and offset
                var transformScale = (cityjsonNode["transform"] != null && cityjsonNode["transform"]["scale"] != null) ? new Vector3Double(
                    cityjsonNode["transform"]["scale"][0].AsDouble,
                    cityjsonNode["transform"]["scale"][1].AsDouble,
                    cityjsonNode["transform"]["scale"][2].AsDouble
                ) : new Vector3Double(1, 1, 1);

                var transformOffset = (cityjsonNode["transform"] != null && cityjsonNode["transform"]["translate"] != null) ? new Vector3Double(
                       cityjsonNode["transform"]["translate"][0].AsDouble,
                       cityjsonNode["transform"]["translate"][1].AsDouble,
                       cityjsonNode["transform"]["translate"][2].AsDouble
                ) : new Vector3Double(0, 0, 0);

                //now load all the vertices with the scaler and offset applied
                foreach (JSONNode node in cityjsonNode["vertices"])
                {
                    var rd = new Vector3RD(
                            node[0].AsDouble * transformScale.x + transformOffset.x,
                            node[1].AsDouble * transformScale.y + transformOffset.y,
                            node[2].AsDouble * transformScale.z + transformOffset.z
                    );
                    var unityCoordinates = CoordConvert.RDtoUnity(rd);
                    allVerts.Add(unityCoordinates);
                }

                //now build the meshes and create objects for these buildings
                int buildingCount = 0;
                foreach (JSONNode buildingNode in cityjsonNode["CityObjects"])
                {
                    var name = buildingNode["attributes"]["identificatie"].Value.Replace("NL.IMBAG.Pand.", "");
                    GameObject building = new GameObject();
                    building.transform.SetParent(this.transform, false);
                    building.name = name;
                    var boundaries = buildingNode["geometry"][lodSlot]["boundaries"][0];
                    meshTriangles = new List<int>();
                    List<Vector3> thisMeshVerts = new List<Vector3>();

                    foreach (JSONNode boundary in boundaries)
                    {
                        JSONNode triangle = boundary[0];

                        vertIndex = triangle[2].AsInt;
                        thisMeshVerts.Add(allVerts[vertIndex]);
                        meshTriangles.Add(thisMeshVerts.Count - 1); //TODO. Group same verts

                        vertIndex = triangle[1].AsInt;
                        thisMeshVerts.Add(allVerts[vertIndex]);
                        meshTriangles.Add(thisMeshVerts.Count - 1);

                        vertIndex = triangle[0].AsInt;
                        thisMeshVerts.Add(allVerts[vertIndex]);
                        meshTriangles.Add(thisMeshVerts.Count - 1);
                    }

                    //Construct the mesh
                    Mesh buildingMesh = new Mesh();
                    buildingMesh.vertices = thisMeshVerts.ToArray();
                    buildingMesh.triangles = meshTriangles.ToArray();
                    buildingMesh.RecalculateNormals();

                    building.AddComponent<MeshRenderer>().enabled = false;
                    building.AddComponent<MeshFilter>().sharedMesh = buildingMesh;
                    buildingCount++;
                }

                parsed++;
                print("Parsed GeoJSONS to fill tile: " + parsed + ". Buildings in tile: " + buildingCount);
            }
        }
    }
}
#endif