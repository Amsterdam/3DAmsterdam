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
    public class ImportBAG3DGeoJson : MonoBehaviour
    {
        [Header("Bounding box in RD coordinates")]
        [SerializeField]
        private Vector2 boundingBoxBottomLeft;
        [SerializeField]
        private Vector2 boundingBoxTopRight;

        [Tooltip("Width and height in meters")]
        [SerializeField]
        private int tileSize = 1000; //1x1 km

        [Tooltip("LOD level name")]
        [SerializeField]
        private double lodLevel = 2.2;

        [Tooltip("LOD slot (index order in the GeoJSON file)")]
        [SerializeField]
        private int lodSlot = 2;

        private List<Vector3> allVerts;
        private List<int> meshTriangles;
        private int vertIndex;

        public Material DefaultMaterial;

        [SerializeField]
        private string geoJsonSourceFilesFolder = "C:/Users/Sam/Desktop/downloaded_tiles_amsterdam/";
        private string unityMeshAssetFolder = "Assets/3DAmsterdam/GeneratedTileAssets/";

        [Header("Tile generation settings")]
        [SerializeField]
        private bool skipExistingFiles = true;
        [SerializeField]
        private bool renderInViewport = true;
        [SerializeField]
        private bool generateAssetFiles = false;
        [SerializeField]
        private bool allowEmptyTileGeneration = false;

        [Tooltip("Remove children not inside a tile, to start with a clean slate for the next tile.")]
        [SerializeField]
        private bool removeChildrenOutsideTile = true;

        [SerializeField]
        private Vector2 tileOffset;
      
        private string previewBackdropImage = "https://geodata.nationaalgeoregister.nl/luchtfoto/rgb/wms?styles=&layers=Actueel_ortho25&service=WMS&request=GetMap&format=image%2Fpng&version=1.1.0&bbox={xmin},{ymin},{xmax},{ymax}&width={w}&height={h}&srs=EPSG:28992";   
        private Texture2D backDropTexture;
        private Texture2D drawIntoPixels;

        [Header("Progress preview")]
        [SerializeField]
        private int backgroundSize = 500;

        [SerializeField]
        private RawImage backgroundRawImage;

        [SerializeField]
        private RawImage gridPixelsRawImage;

        [Header("Chaining other exports")]
        [SerializeField]
        private GameObject optionalObjectToEnableWhenFinished;

        private List<GameObject> overrideChildObjects;

		public void Start()
		{
			//Make sure our tile assets folder is there
			var exportPath = Application.dataPath + "/../" + unityMeshAssetFolder;
			if (!Directory.Exists(exportPath))
			{
				Directory.CreateDirectory(exportPath);
			}

			FindCustomOverrideObjects();

			StartCoroutine(CreateTilesAndReadInGeoJSON());
		}

        /// <summary>
        /// Get override objects added to this object (these will be skipped by the parser, and use the manualy added replacement building).
        /// This is a perfect way to replace specific key buildings with higher detail models.
        /// </summary>
		private void FindCustomOverrideObjects()
		{
			overrideChildObjects = new List<GameObject>();
			foreach (Transform child in transform)
				overrideChildObjects.Add(child.gameObject);
		}

		private void Update()
        {
            if (Input.GetKeyUp(KeyCode.X))
            {
                StopAllCoroutines();
                print("Aborted");
            }
        }

        /// <summary>
        /// This method creates gameobjects for tiles, and fills them up with parsed buildings from GeoJSON files overlapping this tile.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CreateTilesAndReadInGeoJSON()
        {
            print("Baking objects into tiles");
            var xTiles = Mathf.RoundToInt(((float)boundingBoxTopRight.x - (float)boundingBoxBottomLeft.x) / (float)tileSize);
            var yTiles = Mathf.RoundToInt(((float)boundingBoxTopRight.y - (float)boundingBoxBottomLeft.y) / (float)tileSize);

            var totalTiles = xTiles * yTiles;
            int currentTile = 0;

            //Show a previewmap
            backDropTexture = new Texture2D(500, 500, TextureFormat.RGBA32, false);
            drawIntoPixels = new Texture2D(yTiles, yTiles, TextureFormat.RGBA32, false);
            drawIntoPixels.filterMode = FilterMode.Point;

            gridPixelsRawImage.texture = drawIntoPixels;

            //Download background preview image
            var downloadUrl = previewBackdropImage.Replace("{xmin}", boundingBoxBottomLeft.x.ToString()).Replace("{ymin}", boundingBoxBottomLeft.y.ToString()).Replace("{xmax}", boundingBoxTopRight.x.ToString()).Replace("{ymax}", boundingBoxTopRight.y.ToString()).Replace("{w}", backgroundSize.ToString()).Replace("{h}", backgroundSize.ToString());
            print(downloadUrl);
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(downloadUrl);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                backDropTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            }
            backgroundRawImage.texture = backDropTexture;

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
                        if(!Application.isBatchMode) yield return new WaitForEndOfFrame();
                        continue;
                    }

                    //Spawn our tile container
                    GameObject newTileContainer = new GameObject();
                    newTileContainer.transform.position = CoordConvert.RDtoUnity(tileRD + tileOffset);
                    newTileContainer.name = tileName;

                    print("Parsing JSON files for tile " + currentTile + " / " + totalTiles + ": " + newTileContainer.name);
                    if (!Application.isBatchMode) yield return new WaitForEndOfFrame();

                    //Load GEOJsons that overlap this tile
                    yield return StartCoroutine(ParseSpecificFiles(tileRD));

                    if (!Application.isBatchMode) yield return new WaitForEndOfFrame();

                    //Now move them into the tile if their centerpoint is within our defined tile region
                    int buildingsAdded = MoveChildrenIntoTile(tileRD, newTileContainer, true);
                    if (buildingsAdded == 0)
                    {
                        drawIntoPixels.SetPixel(x, y, Color.black);
                    }
                    else
                    {
                        drawIntoPixels.SetPixel(x, y, Color.clear);
                    }
                    drawIntoPixels.Apply();

                    if (!Application.isBatchMode) yield return new WaitForEndOfFrame();

                    //Now bake the tile into an asset file
                    if (generateAssetFiles)
                    {
                        CreateBuildingTile(newTileContainer, newTileContainer.transform.position);
                        print("Created tile " + currentTile + "/" + totalTiles + " with " + buildingsAdded + " buildings -> " + newTileContainer.name);
                    }
                    if (!Application.isBatchMode) yield return new WaitForEndOfFrame();
                }
            }
            print("Done!");
            if (optionalObjectToEnableWhenFinished)
                optionalObjectToEnableWhenFinished.SetActive(true);

        }

        /// <summary>
        /// Moves child buildings whose centerpoint is within the bounds into the tile.
        /// </summary>
        /// <param name="rd">The tile RD coordinates (bottomleft)</param>
        /// <param name="targetParentTile">Tile gameobject to move buildings into</param>
        /// <param name="removeOutside">Clear up the buildings that are outside</param>
        /// <returns></returns>
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
                else if(removeOutside && !overrideChildObjects.Contains(building.gameObject)){
                    //This child is not in our tile. destroy it. Leave it there if it is an override object
                    Destroy(building.GetComponent<MeshFilter>().sharedMesh);
                    Destroy(building.gameObject);

                }
            }
            return buildingsAdded;
        }

        /// <summary>
        /// Combine all the children of this tile into a single mesh
        /// </summary>
        /// <param name="buildingTile">Target tile</param>
        /// <param name="worldPosition">Original position to move the tile to for previewing it</param>
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
            if (totalVertexCount > Mathf.Pow(2, 16))
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

        /// <summary>
        /// Parse files whose name contains coordinates that overlap the provided tile area
        /// </summary>
        /// <param name="rdCoordinates">Bottom left RD coordinates of the tile</param>
        /// <returns></returns>
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

                //Determine parts of the filename
                var id = fileNameParts[0];
                var count = fileNameParts[1];
                var xmin = double.Parse(fileNameParts[3]);
                var ymin = double.Parse(fileNameParts[4]);
                var xmax = double.Parse(fileNameParts[5]);
                var ymax = double.Parse(fileNameParts[6]);

                //Skip if these filename bounds are not within our selected rectangle
                if (xmin > rdCoordinates.x + tileSize || xmax < rdCoordinates.x || ymin > rdCoordinates.y + tileSize || ymax < rdCoordinates.y)
                {
                    continue;
                }
                Debug.Log("Parsing " + file.Name);
                if (!Application.isBatchMode) yield return new WaitForEndOfFrame();

                //Parse the file
                var jsonstring = File.ReadAllText(file.FullName);
                var cityjsonNode = JSON.Parse(jsonstring);
                if (cityjsonNode["CityObjects"] == null)
                {
                    Debug.Log("FAILURE PARSING: " + file.Name);
                    continue; //Failed to parse the json
                }

                //Get vertices
                allVerts = new List<Vector3>();

                //Optionaly parse transform scale and offset
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

                //Now load all the vertices with the scaler and offset applied
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

                //Now build the meshes and create objects for these buildings
                int buildingCount = 0;
                foreach (JSONNode buildingNode in cityjsonNode["CityObjects"])
                {
                    //A building
                    var name = buildingNode["attributes"]["identificatie"].Value.Replace("NL.IMBAG.Pand.", "");

                    //Check if this name/ID exists in our list of manualy added child objects. If it is there, skip it.
                    if(overrideChildObjects.Where(overrideGameObject => overrideGameObject.name == name).SingleOrDefault())
                    {
                        print("Skipped parsing " + name + " because we have added a custom object for that");
                        continue;
					}

                    GameObject building = new GameObject();
                    building.transform.SetParent(this.transform, false);
                    building.name = name;

                    //The building verts/triangles
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
                    if (thisMeshVerts.Count > Mathf.Pow(2, 16))
                        buildingMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

                    buildingMesh.vertices = thisMeshVerts.ToArray();
                    buildingMesh.triangles = meshTriangles.ToArray();
                    buildingMesh.RecalculateNormals();

                    var meshRenderer = building.AddComponent<MeshRenderer>();
                    meshRenderer.material = DefaultMaterial;
                    meshRenderer.enabled = renderInViewport;
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