#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;
using System.IO;
using SimpleJSON;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.Scripting;

namespace Netherlands3D.AssetGeneration.CityJSON
{
    public enum terrainType
    {
        voetpad = 0,
        fietspad = 1,
        parkeervakken = 2,
        wegen = 3,
        begroeid=4,
        erven=5,
        onbegroeid = 6,
        spoorbanen=7,
        woonerven=8,
        constructies =9,
        bruggen=10,
        water = 11,
        anders=99
    }

    public class CityObject
    {
        public List<Vector3RD> vertices;
        public terrainType type;
        public List<int> indices;

        public Dictionary<Vector2, List<Vector3RD>> triangleLists;
        public void GenerateTriangleLists( int tileSize, Vector4 totalBoundingBox)
        {
           triangleLists = new Dictionary<Vector2, List<Vector3RD>>();

            //read all the vertices
            //vertices = new List<Vector3RD>();
            //for (int i = 0; i < indices.Count; i++)
            //{
            //    vertices.Add(allvertices[indices[i]]);
            //}

            Vector2 tileId;
            //check if entire object is inside one tile
                    List<Vector2> overlappingTiles = getOverlappingTiles(vertices, tileSize);
                    foreach (var item in overlappingTiles)
                    {
                    //if (item.x >= totalBoundingBox.x && item.x <= totalBoundingBox.z && item.y >= totalBoundingBox.y && item.y <= totalBoundingBox.w)
                    //    {
                        if (!triangleLists.ContainsKey(item))
                        {
                        
                            triangleLists.Add(item, new List<Vector3RD>());
                        }
                    //}
                }
            
        }

        private List<Vector2> getOverlappingTiles(List<Vector3RD> bboxVertices, int tileSize)
        {
            List<Vector2> overlappingTiles = new List<Vector2>();
            // get boundingbox of total cityObject
            double Xmin = bboxVertices.Min(x => x.x);
            Xmin = Xmin - (Xmin % tileSize) - tileSize;
            double Xmax = bboxVertices.Max(x => x.x);
            Xmax = Xmax - (Xmax % tileSize) + (2*tileSize);
            double Ymin = bboxVertices.Min(x => x.y);
            Ymin = Ymin - (Ymin % tileSize) - tileSize;
            double Ymax = bboxVertices.Max(x => x.y);
            Ymax = Ymax - (Ymax % tileSize) + (2*tileSize);
            Vector2 tile = vertexTile(Xmin, Ymin, tileSize);
            for (int x = (int)Xmin; x < (int)Xmax; x+=tileSize)
            {
                for (int y = (int)Ymin; y < (int)Ymax; y+=tileSize)
                {
                    overlappingTiles.Add( vertexTile(x, y, tileSize));
                }
            }
            return overlappingTiles;
        }
        public bool IsBoundingBoxInSingleTile(List<Vector3RD> bboxVertices, int tileSize, out Vector2 tileID)
        {
            
            // get boundingbox of total cityObject
            double Xmin = bboxVertices.Min(x => x.x);
            double Xmax = bboxVertices.Max(x => x.x);
            double Ymin = bboxVertices.Min(x => x.y);
            double Ymax = bboxVertices.Max(x => x.y);
            Vector2 tile = vertexTile(Xmin, Ymin, tileSize);
            tileID = tile;
            Vector2 tile2 = vertexTile(Xmax, Ymin, tileSize);
            if (tile2 == tile){ return false;}
            tile2 = vertexTile(Xmax, Ymax, tileSize);
            if (tile2 == tile) { return false; }
            tile2 = vertexTile(Xmin, Ymax, tileSize);
            if (tile2 == tile) { return false; }
            return true;
        }

        private Vector2 vertexTile(double x, double y, int tileSize)
        {
            Vector2 tileIndex = new Vector2
                (
                (float)(x),
                (float)(y )                
                ) ;
            return tileIndex;
        }


    }

    public class importTerrain : MonoBehaviour
    {
        
        [Tooltip("Width and height in meters")]
        [SerializeField]
        private int tileSize = 1000; //1x1 km

        [SerializeField]
        private int minimumHeight = -10; //1x1 km
        [SerializeField]
        private int maximumHeight = 25; //1x1 km

        [SerializeField]
        private string geoJsonSourceFilesFolder = "D:/3DRotterdam/Terrain/cityjson";
        //private string unityMeshAssetFolder = "Assets/GeneratedTileAssets/";

        [SerializeField]
        ImportCityJsonTerrain importCityjsonterrainScript;
        [SerializeField]
        private CityJSONterrainParser cityjsonTerrainParser;

        private bool bewerkingGereed = true;
        private CityObject[] cityObjects;
        // Start is called before the first frame update
        void Start()
        {
            importCityjsonterrainScript.heightMin = minimumHeight;
            importCityjsonterrainScript.heightMax = maximumHeight;
            List<string> foldernames = GetFileList(geoJsonSourceFilesFolder);
            Debug.Log(foldernames.Count + " files");

            StartCoroutine(loopthrougFiles(foldernames));
        }

        IEnumerator loopthrougFiles(List<string> folderNames)
        {
            int counter = 0;
            foreach (var item in folderNames)
            {
                yield return new WaitForSeconds(1f);
                counter++;
                
                
                Debug.Log("file " + counter + " van " + folderNames.Count);
                yield return null;
                bewerkingGereed = false;
                
                StartCoroutine(ReadJSONFile(item));
                yield return new WaitWhile(() => bewerkingGereed == false);
                for (int i = 0; i < 20; i++)
                {
                    System.GC.Collect();

                }

            }
            Debug.Log("gereed");
            for (int i = 0; i < 20; i++)
            {
                System.GC.Collect();

            }

        }

        private void moveFile(string fileName)
        {
            if (!System.IO.Directory.Exists(geoJsonSourceFilesFolder+"/gereed"))
            {
                System.IO.Directory.CreateDirectory(geoJsonSourceFilesFolder + "/gereed");
            }

            string[] filePath = fileName.Split('\\');
            string filename = filePath[filePath.Length-1];

            System.IO.Directory.Move(geoJsonSourceFilesFolder + "/" + filename, geoJsonSourceFilesFolder + "/gereed/" + filename);
        }

        
        List<string> GetFileList(string folder)
        {
            var info = new DirectoryInfo(folder);
            var fileInfo = info.GetFiles();

            int numberofFilestoRead = fileInfo.Length;
            //for testing
            //numberofFilestoRead = 1;
            List<string> filenames = new List<string>();
            for (int i = 0; i < numberofFilestoRead; i++)
            {
                if (fileInfo[i].Name.Contains(".json"))
                {
                    filenames.Add(fileInfo[i].FullName);
                }
            }
            return filenames;
        }

 

        private Vector4 getJSONbounds(Vector3RD[] vertices)
        {
            Vector4 totalBoundingBox = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
            double Xmin = vertices.Min(x => x.x);
            Xmin = Xmin - (Xmin % tileSize);
            double Xmax = vertices.Max(x => x.x);
            Xmax = Xmax + (2*tileSize) - (Xmax % tileSize);
            double Ymin = vertices.Min(x => x.y);
            Ymin = Ymin - (Ymin % tileSize);
            double Ymax = vertices.Max(x => x.y);
            Ymax = Ymax + (2*tileSize )- (Ymax % tileSize);

            if (Xmin < totalBoundingBox.x) { totalBoundingBox.x = (float)Xmin; }
            if (Ymin < totalBoundingBox.y) { totalBoundingBox.y = (float)Ymin; }
            if (Xmax > totalBoundingBox.z) { totalBoundingBox.z = (float)Xmax; }
            if (Ymax > totalBoundingBox.w) { totalBoundingBox.w = (float)Ymax; }
            return totalBoundingBox;
        }

        IEnumerator ReadJSONFile(string file)
        {
            cityjsonTerrainParser = new CityJSONterrainParser();
            Debug.Log("parsing file");
            yield return null;
            cityjsonTerrainParser.Parse(file);
            Debug.Log("file parsed");
            yield return null;
            Vector4 totalBoundingBox = getJSONbounds(cityjsonTerrainParser.vertices.ToArray());
            cityjsonTerrainParser.vertices = null;

            cityObjects = cityjsonTerrainParser.cityobjects.ToArray();
            cityjsonTerrainParser.cityobjects = null;
            cityjsonTerrainParser = null;
            for (int i = 0; i < 20; i++) { System.GC.Collect(); }

            Dictionary<Vector2, List<CityObject>> Tiles = new Dictionary<Vector2, List<CityObject>>();

                Debug.Log("number of cityobjects: ("+cityObjects.Length+")");
                yield return null;
                int total = cityObjects.Count();
                int counter = 0;
                Parallel.ForEach(cityObjects, cityObject => { cityObject.GenerateTriangleLists( tileSize,totalBoundingBox); });

                Debug.Log("create tiles");
                yield return null;
            int cityobjectcount = cityObjects.Length;
            int Xmin = (int)totalBoundingBox.x;
            int Ymin = (int)totalBoundingBox.y;
            int Xmax = (int)totalBoundingBox.z;
            int Ymax = (int)totalBoundingBox.w;
            for (int X = Xmin; X < Xmax; X+=tileSize)
            {
                for (int Y = Ymin; Y < Ymax; Y+=tileSize)
                {
                    for (int i = 0; i < 20; i++) { System.GC.Collect(); }
                    //yield return new WaitForSeconds(1f);
                    Debug.Log("create tile " + X + "-" + Y);
                    yield return null;
                    FindCityObjectsForTile(new Vector2(X, Y));
                    
                }
            }
            cityObjects = null;
            Resources.UnloadUnusedAssets();



            for (int i = 0; i < 20; i++) { System.GC.Collect(); }
            moveFile(file);
            //moveFolder(foldername);
            bewerkingGereed = true;
        }


        private void FindCityObjectsForTile(Vector2 tileKey)
        {
            //Dictionary<terrainType, CityObject> tileCityObjects = new Dictionary<terrainType, CityObject>();
            int cityobjectcount = cityObjects.Length;
            int enumIndex;
            List<Vector3RD> vertices = new List<Vector3RD>();
            int[] terraintypeSizes = Enumerable.Range(0, 12).ToArray();
            // find the total number of vertices for each terraintype in the tile, so we can set the arraysize in advance.
            for (int i = 0; i < cityobjectcount; i++)
            {
                if (!cityObjects[i].triangleLists.ContainsKey(tileKey))
                {
                    continue;
                }
                enumIndex = (int)cityObjects[i].type;

                if (enumIndex<12)
                {
                    terraintypeSizes[enumIndex]+=cityObjects[i].vertices.Count;
                }
            }


            List<Mesh> meshes = new List<Mesh>();
            for (int i = 0; i < 12; i++)
            {
                terraintypeSizes[i]++;
                vertices.Capacity = terraintypeSizes[i];
                for (int coI = 0; coI < cityobjectcount; coI++)
                {
                    if (cityObjects[coI].triangleLists.ContainsKey(tileKey))
                    {
                        if ((int)cityObjects[coI].type == i)
                        {
                            vertices.AddRange(cityObjects[coI].vertices);
                        }
                    }

                }
                //create submesh
                Mesh mesh = importCityjsonterrainScript.CreateCityObjectMesh(vertices, tileKey.x, tileKey.y, tileSize); ;
                vertices.Clear();
                if (i == (int)terrainType.begroeid || i == (int)terrainType.erven || i == (int)terrainType.onbegroeid)
                {
                    importCityjsonterrainScript.SimplifyMesh(ref mesh, 0.05f);
                }

                meshes.Add(mesh);
            }
            importCityjsonterrainScript.CreateCombinedMeshes(ref meshes, tileKey, tileSize);
            for (int i = 0; i < meshes.Count; i++)
            {
                DestroyImmediate(meshes[i], true);
            }
            meshes = null;
            for (int i = 0; i < 20; i++) { System.GC.Collect(); }

            
        }



    }
    public class TerrainFilter
    {
        public List<string> RoadsVoetpad = new List<string> { "voetpad", "voetgangersgebied", "ruiterpad", "voetpad op trap" };
        public string RoadsVoetpadPropertyName = "bgt_functie";

        public List<string> RoadsFietspad = new List<string> { "fietspad" };
        public string RoadsFietsPropertyName = "bgt_functie";

        public List<string> RoadsParkeervak = new List<string> { "parkeervlak" };
        public string RoadsParkeervakPropertyName = "bgt_functie";

        public List<string> RoadsSpoorbaan = new List<string> { "spoorbaan" };
        public string RoadsSpoorbaanPropertyName = "bgt_functie";

        public List<string> RoadsWoonerf = new List<string> { "transitie", "woonerf" };
        public string RoadsWoonerfPropertyName = "bgt_functie";
 
        public terrainType roadsOverig = terrainType.wegen;

        //                Mesh LandUseVoetpadMesh = CreateCityObjectMesh(cm, "LandUse", originX, originY, tileSize, "bgt_fysiekvoorkomen", new List<string> { "open verharding" }, true);
        public List<string> LandUseVoetpad = new List<string> { "open verharding" };
        public string LandUseVoetpadPropertyName = "bgt_fysiekvoorkomen";

        public List<string> LandUseRoads = new List<string> { "gesloten verharding" };
        public string LandUseRoadsPropertyName = "bgt_fysiekvoorkomen";

        public List<string> LandUseGroen = new List<string> { "groenvoorziening" };
        public string LandUseGroenPropertyName = "bgt_fysiekvoorkomen";

        public List<string> LandUseErf = new List<string> { "erf" };
        public string LandUseErfPropertyName = "bgt_fysiekvoorkomen";

        public terrainType landUseOverig = terrainType.onbegroeid;
    }
}

#endif