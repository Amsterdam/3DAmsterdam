#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Netherlands3D.Core;
using System.IO;
using System.Threading;
using Netherlands3D.TileSystem;
using System.Linq;
using SimpleJSON;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace Netherlands3D.AssetGeneration.CityJSON
{
    public class ImportBAG3DGeoJson : MonoBehaviour
    {

        [Tooltip("Width and height in meters")]
        [SerializeField]
        private int tileSize = 1000; //1x1 km

        [Tooltip("LOD level name")]
        [SerializeField]
        private string lodLevel = "2.2";

        [Tooltip("LOD slot (index order in the GeoJSON file)")]
        [SerializeField]
        private int lodSlot = 2;

        private List<Vector3> allVerts;
        private List<int> meshTriangles;
        private int vertIndex;

        [SerializeField]
        private string geoJsonSourceFilesFolder = "C:/Users/Sam/Desktop/downloaded_tiles_amsterdam/";

        [Header("Tile generation settings")]
        [SerializeField]
        private bool skipExistingFiles = true;

        [SerializeField]
        private Material defaultMaterial;

        [SerializeField]
        private bool renderInViewport = true;

        [SerializeField]
        private bool generateAssetFiles = false;
        [SerializeField]
        private bool allowEmptyTileGeneration = false;

        [Header("Optional. Leave blank to create all tiles")]
        [SerializeField]
        private string exclusivelyGenerateTilesWithSubstring = "";

        [Tooltip("Remove children not inside a tile, to start with a clean slate for the next tile.")]
        [SerializeField]
        private bool removeChildrenOutsideTile = true;

        [SerializeField]
        private Vector2 tileOffset;

        private WeldMeshVertices vertexWelder;

        [Header("Chaining other exports")]
        [SerializeField]
        private GameObject optionalObjectToEnableWhenFinished;

        private List<GameObject> overrideChildObjects;

        public void Start()
        {
            //Check if bounding box uses coordinates that are in thousands
            if (!Config.activeConfiguration.BottomLeftRD.IsInThousands || !Config.activeConfiguration.TopRightRD.IsInThousands)
            {
                print($"<color=#ff0000>Bounding box should be in thousands</color>");
                return;
            }

            vertexWelder = this.gameObject.AddComponent<WeldMeshVertices>();
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
            var xTiles = Mathf.RoundToInt(((float)Config.activeConfiguration.TopRightRD.x - (float)Config.activeConfiguration.BottomLeftRD.x) / (float)tileSize);
            var yTiles = Mathf.RoundToInt(((float)Config.activeConfiguration.TopRightRD.y - (float)Config.activeConfiguration.BottomLeftRD.y) / (float)tileSize);

            var totalTiles = xTiles * yTiles;
            int currentTile = 0;

            yield return ProgressPreviewMap.Instance.Initialize(xTiles, yTiles);

            //Walk the tilegrid
            var tileRD = new Vector2Int(0, 0);
            for (int x = 0; x < xTiles; x++)
            {
                tileRD.x = (int)Config.activeConfiguration.BottomLeftRD.x + (x * tileSize);
                for (int y = 0; y < yTiles; y++)
                {
                    currentTile++;

                    tileRD.y = (int)Config.activeConfiguration.BottomLeftRD.y + (y * tileSize);

                    string tileName = "buildings_" + tileRD.x + "_" + tileRD.y + "." + lodLevel;

                    //If we supplied a filter we check if this tile contains this substring in order to be (re)generated
                    if (exclusivelyGenerateTilesWithSubstring != "" && !tileName.Contains(exclusivelyGenerateTilesWithSubstring))
                    {
                        print("Skipping tile because we supplied a specific name we want to replace.");
                        if (!Application.isBatchMode) yield return new WaitForEndOfFrame();
                        continue;
                    }

                    //Skip files if we enabled that option and it exists
                    string assetFileName = TileCombineUtility.unityMeshAssetFolder + tileName + ".asset";
                    if (skipExistingFiles && File.Exists(Application.dataPath + "/../" + assetFileName))
                    {
                        print("Skipping existing tile: " + Application.dataPath + "/../" + assetFileName);
                        ProgressPreviewMap.Instance.ColorTile(x, y, TilePreviewState.SKIPPED);

                        if (!Application.isBatchMode) yield return new WaitForEndOfFrame();
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

                    ProgressPreviewMap.Instance.ColorTile(x, y, (buildingsAdded == 0) ? TilePreviewState.EMPTY : TilePreviewState.DONE);

                    if (!Application.isBatchMode) yield return new WaitForEndOfFrame();

                    //Now bake the tile into an asset file
                    if (generateAssetFiles)
                    {
                        TileCombineUtility.CombineSource(newTileContainer, newTileContainer.transform.position, renderInViewport, defaultMaterial, generateAssetFiles);
                        print("Created tile " + currentTile + "/" + totalTiles + " with " + buildingsAdded + " buildings -> " + newTileContainer.name);
                    }
                    if (!Application.isBatchMode) yield return new WaitForEndOfFrame();
                }
            }
            Debug.Log(this.name + " is done!", this.gameObject);
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
                else if (removeOutside && !overrideChildObjects.Contains(building.gameObject))
                {
                    //This child is not in our tile. destroy it. Leave it there if it is an override object
                    Destroy(building.GetComponent<MeshFilter>().sharedMesh);
                    Destroy(building.gameObject);

                }
            }
            return buildingsAdded;
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
                if (cityjsonNode == null || cityjsonNode["CityObjects"] == null)
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
                    if (overrideChildObjects.Where(overrideGameObject => overrideGameObject != null && overrideGameObject.name == name).SingleOrDefault())
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

                    buildingMesh = vertexWelder.WeldVertices(buildingMesh);

                    var meshRenderer = building.AddComponent<MeshRenderer>();
                    meshRenderer.material = defaultMaterial;
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