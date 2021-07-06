#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
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
        Netherlands3D.AssetGeneration.CityJSON.ImportCityJsonTerrain importCityjsonterrainScript;
        private TerrainFilter terrainFilter = new TerrainFilter();
        private bool bewerkingGereed = true;

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
                counter++;
                
                
                Debug.Log("file " + counter + " van " + folderNames.Count);
                bewerkingGereed = false;
                
                StartCoroutine(ReadJSONFile(item));
                yield return new WaitWhile(() => bewerkingGereed == false);
                for (int i = 0; i < 20; i++)
                {
                    System.GC.Collect();

                }

            }

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

        private void moveFolder(string foldername)
        {
            string newfoldername = foldername.Replace("original","gereed");
            Directory.Move(foldername, newfoldername);
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

        List<string> GetFolders()
        {
            List<string> foldernames = new List<string>();
            var info = new DirectoryInfo(geoJsonSourceFilesFolder);
            var fileInfo = info.GetDirectories();
            for (int i = 0; i < fileInfo.Length; i++)
            {
                foldernames.Add(fileInfo[i].FullName);
            }
            return foldernames;
        }

        private Vector4 getJSONbounds(Vector3RD[] vertices)
        {
            Vector4 totalBoundingBox = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
            double Xmin = vertices.Min(x => x.x);
            Xmin = Xmin - (Xmin % tileSize);
            double Xmax = vertices.Max(x => x.x);
            Xmax = Xmax + tileSize - (Xmax % tileSize);
            double Ymin = vertices.Min(x => x.y);
            Ymin = Ymin - (Ymin % tileSize);
            double Ymax = vertices.Max(x => x.y);
            Ymax = Ymax +tileSize - (Ymax % tileSize);

            if (Xmin < totalBoundingBox.x) { totalBoundingBox.x = (float)Xmin; }
            if (Ymin < totalBoundingBox.y) { totalBoundingBox.y = (float)Ymin; }
            if (Xmax > totalBoundingBox.z) { totalBoundingBox.z = (float)Xmax; }
            if (Ymax > totalBoundingBox.w) { totalBoundingBox.w = (float)Ymax; }
            return totalBoundingBox;
        }

        IEnumerator ReadJSONFile(string file)
        {

            //List<string> filenames = GetFileList(foldername);
           
            
                        
           Debug.Log("reading file: " + file);
           yield return null;
            var jsonstring = File.ReadAllText(file);
            Debug.Log("parsing file: " + file);
            yield return null;
            JSONNode cityModel = JSON.Parse(jsonstring);
            jsonstring = "";
            for (int i = 0; i < 10; i++)
            {
                System.GC.Collect();

            }
            Debug.Log("reading vertices");
                yield return null;
                Vector3RD[] vertices = readVertices(cityModel);
            Vector4 totalBoundingBox = getJSONbounds(vertices);

            //loop through cityobjects
            Debug.Log("collecting cityobjects");
                yield return null;
                CityObject[] cityObjects = GetCityObjects(cityModel["CityObjects"],vertices);
                
            vertices = null;
                cityModel = null;
            

            for (int i = 0; i < 20; i++)
            {
                System.GC.Collect();

            }

            Dictionary<Vector2, List<CityObject>> Tiles = new Dictionary<Vector2, List<CityObject>>();

                Debug.Log("find cityobjectlocations("+cityObjects.Length+")");
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
                    FindCityObjectsForTile(new Vector2(X, Y), ref cityObjects);
                    Debug.Log("create tile " + X + "-" + Y);
                    yield return null;
                }
            }

            cityObjects = new CityObject[0];

           
            for (int i = 0; i < 20; i++) { System.GC.Collect(); }
            moveFile(file);
            //moveFolder(foldername);
            bewerkingGereed = true;
        }


        private void FindCityObjectsForTile(Vector2 tileKey, ref CityObject[] cityObjects)
        {
            Dictionary<terrainType, CityObject> tileCityObjects = new Dictionary<terrainType, CityObject>();
            int cityobjectcount = cityObjects.Length;
            Dictionary<terrainType, int> terraintypeSizes = new Dictionary<terrainType, int>();
            // find the total number of vertices for each terraintype in the tile, so we can set the arraysize in advance.
            for (int i = 0; i < cityobjectcount; i++)
            {
                if (!cityObjects[i].triangleLists.ContainsKey(tileKey))
                {
                    continue;
                }
                if (terraintypeSizes.ContainsKey(cityObjects[i].type))
                {
                    terraintypeSizes[cityObjects[i].type]+=cityObjects[i].vertices.Count;
                }
                else
                {
                    terraintypeSizes.Add(cityObjects[i].type, cityObjects[i].vertices.Count);
                }
            }





            for (int i = 0; i < cityobjectcount; i++)
            {
                if (!cityObjects[i].triangleLists.ContainsKey(tileKey))
                {
                    continue;
                }
                if (tileCityObjects.ContainsKey(cityObjects[i].type))
                {
                    tileCityObjects[cityObjects[i].type].vertices.AddRange(cityObjects[i].vertices);
                }
                else
                {
                    tileCityObjects.Add(cityObjects[i].type, cityObjects[i]);
                    cityObjects[i].vertices.Capacity = terraintypeSizes[cityObjects[i].type];
                }
            }

            Dictionary<terrainType, Mesh> meshes = new Dictionary<terrainType, Mesh>();
            foreach (var item in tileCityObjects)
            {
                Mesh mesh = importCityjsonterrainScript.CreateCityObjectMesh(item.Value.vertices, tileKey.x, tileKey.y, tileSize);
                if (item.Key == terrainType.begroeid || item.Key == terrainType.erven || item.Key == terrainType.onbegroeid)
                {
                    mesh = importCityjsonterrainScript.SimplifyMesh(mesh, 0.05f);
                }
                meshes.Add(item.Key, mesh);
                item.Value.vertices.Clear();
            }
            tileCityObjects = new Dictionary<terrainType, CityObject>();
            for (int i = 0; i < 20; i++) { System.GC.Collect(); }
            importCityjsonterrainScript.CreateCombinedMeshes(ref meshes, tileKey, tileSize);
            foreach (var item in meshes.Values)
            {
                DestroyImmediate(item, true);
            }
            meshes = null;
            for (int i = 0; i < 20; i++) { System.GC.Collect(); }
        }

        Vector3RD[] readVertices(JSONNode citymodel)
        {
            //needs to be sequential
            JSONNode verticesNode = citymodel["vertices"];
            Vector3 transformScale = Vector3.one;
            Vector3RD transformTranslate = new Vector3RD(0, 0, 0);


            if (citymodel["transform"] != null)
            {
                if (citymodel["transform"]["scale"] != null)
                {
                    transformScale = new Vector3
                    (
                        citymodel["transform"]["scale"][0].AsFloat,
                        citymodel["transform"]["scale"][1].AsFloat,
                        citymodel["transform"]["scale"][2].AsFloat
                   );
                }
                if (citymodel["transform"]["translate"] != null)
                {
                    transformTranslate = new Vector3RD
                    (
                        citymodel["transform"]["translate"][0].AsDouble,
                        citymodel["transform"]["translate"][1].AsDouble,
                        citymodel["transform"]["translate"][2].AsDouble
                   );
                }
            }
            
            long vertcount = verticesNode.Count;
            Vector3RD[] vertices = new Vector3RD[vertcount];
            for (int i = 0; i < vertcount; i++)
            {
                Vector3RD vert = new Vector3RD();
                vert.x = (verticesNode[i][0].AsDouble*transformScale.x) + transformTranslate.x;
                vert.y = (verticesNode[i][1].AsDouble * transformScale.y) + transformTranslate.y;
                vert.z = (verticesNode[i][2].AsDouble * transformScale.z) + transformTranslate.z;
                vertices[i]=vert;
            }
            return vertices;
        }

        private CityObject[] GetCityObjects(JSONNode cityobjects, Vector3RD[] vertices)
        {
            CityObject[] cityObjects = new CityObject[cityobjects.Count];
            for (int i = 0; i < cityobjects.Count; i++)
            {
                CityObject cityObject = new CityObject();
                List<int> indices = ReadTriangles(cityobjects[i]);
                List<Vector3RD> coVerts = new List<Vector3RD>();
                for (int index = 0; index < indices.Count; index++)
                {
                    coVerts.Add(vertices[indices[index]]);
                }
                cityObject.vertices = coVerts;
                cityObject.type = getTerrainType(cityobjects[i]);
                cityObjects[i] = cityObject;
            }
            return cityObjects;
        }

        private terrainType getTerrainType(JSONNode cityObject)
        {

            string cityObjectType = cityObject["type"];

            if (cityObjectType == "Road")
            {
                if (terrainFilter.RoadsVoetpad.Contains(cityObject["attributes"][terrainFilter.RoadsVoetpadPropertyName]))
                {
                    return terrainType.voetpad;
                }
                if (terrainFilter.RoadsFietspad.Contains(cityObject["attributes"][terrainFilter.RoadsFietsPropertyName]))
                {
                    return terrainType.fietspad;
                }
                if (terrainFilter.RoadsParkeervak.Contains(cityObject["attributes"][terrainFilter.RoadsParkeervakPropertyName]))
                {
                    return terrainType.parkeervakken;
                }
                if (terrainFilter.RoadsSpoorbaan.Contains(cityObject["attributes"][terrainFilter.RoadsSpoorbaanPropertyName]))
                {
                    return terrainType.spoorbanen;
                }
                if (terrainFilter.RoadsWoonerf.Contains(cityObject["attributes"][terrainFilter.RoadsWoonerfPropertyName]))
                {
                    return terrainType.woonerven;
                }
                return terrainType.wegen;
            }
            if (cityObjectType == "LandUse")
            {
                if (terrainFilter.LandUseVoetpad.Contains(cityObject["attributes"][terrainFilter.LandUseVoetpadPropertyName]))
                {
                    return terrainType.voetpad;
                }
                if (terrainFilter.LandUseRoads.Contains(cityObject["attributes"][terrainFilter.LandUseRoadsPropertyName]))
                {
                    return terrainType.wegen;
                }
                if (terrainFilter.LandUseGroen.Contains(cityObject["attributes"][terrainFilter.LandUseGroenPropertyName]))
                {
                    return terrainType.begroeid;
                }
                if (terrainFilter.LandUseErf.Contains(cityObject["attributes"][terrainFilter.LandUseErfPropertyName]))
                {
                    return terrainType.erven;
                }
                return terrainType.onbegroeid;
            }
            if (cityObjectType == "PlantCover")
            {
                return terrainType.begroeid;
            }
            if (cityObjectType == "GenericCityObject")
            {
                return terrainType.constructies;
            }
            if (cityObjectType == "WaterBody")
            {
                return terrainType.water;
            }
            if (cityObjectType == "Bridge")
            {
                return terrainType.bruggen;
            }
            if (cityObjectType =="Building")
            {
                return terrainType.anders;
            }
            Debug.Log(cityObjectType);
            return terrainType.anders;
        }

        private List<int> ReadTriangles(JSONNode cityObject)
        {
            List<int> triangles = new List<int>();
            JSONNode boundariesNode = cityObject["geometry"][0]["boundaries"];
            // End if no BoundariesNode
            if (boundariesNode is null)
            {
                return triangles;
            }
            foreach (JSONNode boundary in boundariesNode)
            {
                JSONNode outerRing = boundary[0];
                triangles.Add(outerRing[2].AsInt);
                triangles.Add(outerRing[1].AsInt);
                triangles.Add(outerRing[0].AsInt);
            }

            return triangles;
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