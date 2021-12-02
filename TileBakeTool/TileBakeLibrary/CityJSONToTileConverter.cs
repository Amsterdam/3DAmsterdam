#define DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using TileBakeLibrary.Coordinates;
using Bunny83.SimpleJSON;
using System.Linq;
using Netherlands3D.CityJSON;
using JoeStrout;
using System.Threading;
using System.Collections.Concurrent;

namespace TileBakeLibrary
{
	public class CityJSONToTileConverter
	{
		private string sourcePath = "";
		private string outputPath = "";

        private string identifier = "";
        private string removeFromID = "";

        private bool createOBJFiles = false;
        private bool brotliCompress = false;

        private bool replaceExistingIDs = true;

        private float maxNormalAngle = 5.0f; 

        private bool addToExistingTiles = false;

        private float lod = 0;
        private string filterType = "";

        public string TilingMethod = "Overlap";

        private int tileSize = 1000;

        private List<SubObject> allSubObjects = new List<SubObject>();
        private List<Tile> tiles = new List<Tile>();

		private CityObjectFilter[] cityObjectFilters;

		/// <summary>
		/// The LOD we want to parse. 
		/// </summary>
		/// <param name="targetLOD">Defaults to 0</param>
		public void SetLOD(float targetLOD)
        {
            lod = targetLOD;
        }

        /// <summary>
        /// Define what kind of cityobject type you want to parse
        /// </summary>
        public void SetFilterType(string type)
        {
            filterType = type;
        }

        /// <summary>
        /// Determines the property that will be used as unique object identifier
        /// </summary>
        /// <param name="id">The propery field name, for example 'building_id'</param>
        /// <param name="remove">Remove this substring from the ID before storing it</param>
        public void SetID(string id, string remove)
        {
            identifier = id;
            removeFromID = remove;
        }

        /// <summary>
        /// The source folder path containing all .cityjson files that need to be converted
        /// </summary>
        /// <param name="source"></param>
        public void SetSourcePath(string source)
		{
			sourcePath = source;
		}

		/// <summary>
		/// Target folder where the generated binary tiles should be placed
		/// </summary>
		/// <param name="target"></param>
		public void SetTargetPath(string target)
		{
			outputPath = target;
		}

        /// <summary>
        /// Parse exisiting binary tile files and add the parsed objects to them
        /// </summary>
        /// <param name="add">Add to existing tiles</param>
		public void SetReplace(bool replace)
		{
            this.replaceExistingIDs = replace;
        }

        /// <summary>
        /// Create an OBJ model file next to the binary file
        /// </summary>
		public void CreateOBJ(bool createObjFiles)
		{
            this.createOBJFiles = createObjFiles;
        }
        
        /// <summary>
        /// Create a brotli compressed version of the binary tiles
        /// </summary>
        public void AddBrotliCompressedFile(bool brotliCompress)
		{
            this.brotliCompress = brotliCompress;
        }


        private CityJSON cityJson;

		/// <summary>
		/// Start converting the cityjson files into binary tile files
		/// </summary>
        /// 

		public void Convert()
		{
#if DEBUG
            Console.WriteLine($"Converting with Debug mode ON");
#endif

            //If no specific filename or wildcard was supplied, default to .json files
            var filter = Path.GetFileName(sourcePath);
			if(filter == "") filter = "*.json";

            //List the files that we are going to parse
			string[] sourceFiles = Directory.GetFiles(sourcePath, filter);
            if(sourceFiles.Length == 0){
                Console.WriteLine($"No \"{filter}\" files found in {sourcePath}");
                return;
            }

            //Create a threadable task for every file, that returns a list of parsed cityobjects
            Console.WriteLine($"Parsing {sourceFiles.Length} CityJSON files...");
			for (int i = 0; i < sourceFiles.Length; i++)
			{
                var index = i;
                var cityObjects = CityJSONParseProcess(sourceFiles[index]);
                
                allSubObjects.AddRange(cityObjects);

                BakeTiles();
                allSubObjects.Clear();
            }
		}


		/// <summary>
		/// Bake the city objects into binary tiles
		/// </summary>
		private void BakeTiles()
        {
            //Determine what tiles we will need using our parsed cityobject centroids
            var minX = double.MaxValue;
            var maxX = double.MinValue;

            var minY = double.MaxValue;
            var maxY = double.MinValue;
            foreach (SubObject cityObject in allSubObjects)
            {
                if (cityObject.centroid.X < minX) minX = cityObject.centroid.X;
                else if (cityObject.centroid.X >= maxX) maxX = cityObject.centroid.X;

                if (cityObject.centroid.Y < minY) minY = cityObject.centroid.Y;
                else if (cityObject.centroid.Y >= maxY) maxY = cityObject.centroid.Y;
            }

            //Create our grid of tiles
            var startXRD = Math.Floor(minX / tileSize) * tileSize;
            var startYRD = Math.Floor(minY / tileSize) * tileSize;

            var endXRD = Math.Ceiling(maxX / tileSize) * tileSize;
            var endYRD = Math.Ceiling(maxY / tileSize) * tileSize;

            var XTiles = (endXRD-startXRD) / tileSize;
            var YTiles = (endYRD-startYRD) / tileSize;

            int existingTilesFound = 0;
            tiles.Clear();
            for (int x = 0; x < XTiles; x++)
			{
                var tileX = startXRD + (x * tileSize);
                for (int y = 0; y < YTiles; y++)
                {
                    var tileY = startYRD + (y * tileSize);
                    var newTile = new Tile()
                    {
                        size = new Vector2(tileSize, tileSize),
                        position = new Vector2Double(tileX, tileY),
                        filePath = $"{outputPath}{tileX}_{tileY}.{lod}.bin"
                    };
                    tiles.Add(newTile);

                    //Parse exisiting file
                    if(File.Exists(newTile.filePath))
                    {
                        existingTilesFound++;
                        ParseExistingBinaryTile(newTile);
                    }
                }
            }
            Console.WriteLine($"Baking {XTiles}x{YTiles} = {XTiles*YTiles} tiles");
            if (existingTilesFound > 0)
            {
                Console.WriteLine($"Parsed {existingTilesFound} existing tile files.");
            }

            //Create binary files (if we added subobjects to it)
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            //Move the CityObjects into to the proper tile based on their centroid
            for (int i = allSubObjects.Count - 1; i >= 0; i--)
			{
                var subObject = allSubObjects[i];
                if (removeFromID != "")
                {
                    subObject.id = subObject.id.Replace(removeFromID, "");
                }

                Parallel.ForEach(tiles, tile =>
                {
                    if (subObject.centroid.X > tile.position.X
                    && subObject.centroid.X <= tile.position.X + tileSize
                    && subObject.centroid.Y > tile.position.Y
                    && subObject.centroid.Y <= tile.position.Y + tileSize)
                    {
                        tile.AddSubObject(subObject, replaceExistingIDs);
                    };
                });
            }

            //Threaded writing of binary meshes + compression
            Parallel.ForEach(tiles, tile =>
            {
                if (tile.SubObjects.Count == 0)
                {
                    Console.WriteLine($"Skipping {tile.filePath} containing {tile.SubObjects.Count} SubObjects");
                }
                else
                {
                    //Bake the tile, and lets save it!
                    
                    tile.Bake();
                    Console.WriteLine($"Saving {tile.filePath} containing {tile.SubObjects.Count} SubObjects");
                    
                    //Create binary files
                    BinaryMeshWriter.Save(tile);

                    //Compressed variant
                    if (brotliCompress) BrotliCompress.Compress(tile.filePath);

                    //Optionaly write other format(s) for previewing purposes
                    if (createOBJFiles) OBJWriter.Save(tile);
                }
            });

            Console.WriteLine("Done.");
        }

		public void SetObjectFilters(CityObjectFilter[] cityObjectFilters)
		{
			this.cityObjectFilters = cityObjectFilters;
        }

		/// <summary>
		/// Read SubObjects from existing binary tile file
		/// </summary>
		private void ParseExistingBinaryTile(Tile tile)
		{
             BinaryMeshReader.ReadBinaryFile(tile);
             Console.WriteLine($"Parsed existing tile {tile.filePath} with {tile.SubObjects.Count} subobjects");
        }


        private List<SubObject> CityJSONParseProcess(string sourceFile)
        {
            List<SubObject> filteredObjects = new List<SubObject>();

            Console.WriteLine($"Parsing CityJSON: {sourceFile}");
            Console.Write("loading file...");
            cityJson = new CityJSON(sourceFile, true, true);
            Console.Write("\r reading cityobjects");
            //List<CityObject> cityObjects = cityJson.LoadCityObjects(lod);
            int cityObjectCount = cityJson.CityObjectCount();
            Console.WriteLine($"\r CityObjects found: {cityObjectCount}");
            Console.Write("---");
            int done = 0;
            int parsing = 0;
            int simplifying = 0;
            int tiling = 0;
            var filterobjectsBucket = new ConcurrentBag<SubObject>();
            int[] indices = Enumerable.Range(0, cityObjectCount).ToArray(); ;
            //Turn cityobjects (and their children) into SubObject mesh data
            var partitioner = Partitioner.Create(indices, EnumerablePartitionerOptions.NoBuffering);
            Parallel.ForEach(partitioner, i =>
             {
                 Interlocked.Increment(ref parsing);
                 CityObject cityObject = cityJson.LoadCityObjectByID(i, lod);
                 Console.Write("\r"+ done + " done; "+ parsing + " parsing; "+ simplifying + " simplifying; "+ tiling +" tiling                    ");
                 var subObject = ToSubObjectMeshData(cityObject);
                 cityJson.ClearCityObject(cityObject.keyName);
                 cityObject = null;
                 Interlocked.Decrement(ref parsing);
                 Console.Write("\r" + done + " done; " + parsing + " parsing; " + simplifying + " simplifying; " + tiling + " tiling                    ");
                 cityObject = null;
                 
                 if (subObject.maxVerticesPerSquareMeter > 0)
                 {
                     Interlocked.Increment(ref simplifying);
                     Console.Write("\r" + done + " done; " + parsing + " parsing; " + simplifying + " simplifying; " + tiling + " tiling                    ");
                     subObject.SimplifyMesh();
                     Interlocked.Decrement(ref simplifying);
                     Console.Write("\r" + done + " done; " + parsing + " parsing; " + simplifying + " simplifying; " + tiling + " tiling                    ");
                 }

                 if (TilingMethod == "TILED")
                 {
                     Interlocked.Increment(ref tiling);
                     Console.Write("\r" + done + " done; " + parsing + " parsing; " + simplifying + " simplifying; " + tiling + " tiling                    ");
                     var newSubobjects = subObject.clipSubobject();
                     if (newSubobjects.Count == 0)
                     {
                         filterobjectsBucket.Add(subObject);
                     }
                     else
                     {
                         foreach (var newsubObject in newSubobjects)
                         {
                             if (newsubObject != null)
                             {
                                 filterobjectsBucket.Add(newsubObject);
                             }
                         }
                     }
                     Interlocked.Decrement(ref tiling);
                     Console.Write("\r" + done + " done; " + parsing + " parsing; " + simplifying + " simplifying; " + tiling + " tiling                    ");
                 }
                 
                 Interlocked.Increment(ref done);
                 Console.Write("\r" + done + " done; " + parsing + " parsing; " + simplifying + " simplifying; " + tiling + " tiling                    ");
                 Console.Write("\r" + done);

             }
            );
            Console.WriteLine("file parsed");
            return filterobjectsBucket.ToList();
        }

		private SubObject ToSubObjectMeshData(CityObject cityObject)
		{
            List<SubObject> subObjects = new List<SubObject>();
            var subObject = new SubObject();
            subObject.vertices = new List<Vector3Double>();
            subObject.normals = new List<Vector3>();
            subObject.uvs = new List<Vector2>();
            subObject.triangleIndices = new List<int>();
            subObject.id = cityObject.keyName;



            
            int submeshindex = -1;

            // figure out the intended submesh and required meshDensity
            for (int i = 0; i < cityObjectFilters.Length; i++)
            {
                if (cityObjectFilters[i].objectType == cityObject.cityObjectType)
                {
                    submeshindex = cityObjectFilters[i].defaultSubmeshIndex;
                    subObject.maxVerticesPerSquareMeter = cityObjectFilters[i].maxVerticesPerSquareMeter;
                    for (int j = 0; j < cityObjectFilters[i].attributeFilters.Length; j++)
                    {
                        string attributename = cityObjectFilters[i].attributeFilters[j].attributeName;
                        for (int k = 0; k < cityObjectFilters[i].attributeFilters[j].valueToSubMesh.Length; k++)
                        {
                            string value = cityObjectFilters[i].attributeFilters[j].valueToSubMesh[k].value;
                            for (int l = 0; l < cityObject.semantics.Count; l++)
                            {
                                if (cityObject.semantics[l].name== attributename)
                                {
                                    if (cityObject.semantics[l].value == value)
                                    {
                                        submeshindex = cityObjectFilters[i].attributeFilters[j].valueToSubMesh[k].submeshIndex;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            subObject.parentSubmeshIndex= submeshindex;
            if (submeshindex==-1)
            {
                return new SubObject();
            }


            //If we supplied a specific identifier field, use it as ID instead of object key index
            if (identifier != "")
            {
                foreach (var semantic in cityObject.semantics)
                {
                    //Console.WriteLine(semantic.name);
                    if (semantic.name == identifier)
                    {
                        subObject.id = semantic.value;
                        break;
                    }
                }
            }

            AppendCityObjectGeometry(cityObject, subObject);
            //Append all child geometry too
            for (int i = 0; i < cityObject.children.Count; i++)
			{
				var childObject = cityObject.children[i];
				//Add child geometry to our subobject. (Recursive children are not allowed in CityJson)
				AppendCityObjectGeometry(childObject, subObject);
			}
            if (maxNormalAngle != 0)
            {
                subObject.MergeSimilarVertices(maxNormalAngle);
            }

            

            //Winding order of triangles should be reversed
            subObject.triangleIndices.Reverse();

            //Check if the list if triangles is complete (divisible by 3)
            if (subObject.triangleIndices.Count % 3 != 0)
            {
                Console.WriteLine($"{subObject.id} triangle list is not divisible by 3. This is not correct.");
            }

            //Calculate centroid using the city object vertices
            Vector3Double centroid = new Vector3Double();
            for (int i = 0; i < subObject.vertices.Count; i++)
			{
                centroid.X += subObject.vertices[i].X;
                centroid.Y += subObject.vertices[i].Y;
            }
            subObject.centroid = new Vector2Double(centroid.X / subObject.vertices.Count, centroid.Y / subObject.vertices.Count);


           


            return subObject;
        }

		private static void AppendCityObjectGeometry(CityObject cityObject, SubObject subObject)
		{
            List<Vector3Double> vertexlist = new List<Vector3Double>();
            Vector3 defaultNormal = new Vector3(0, 1, 0);
            List<Vector3> defaultnormalList = new List<Vector3> { defaultNormal, defaultNormal, defaultNormal };
            List<Vector3> normallist = new List<Vector3>();
            List<int> indexlist = new List<int>();
            int count = subObject.vertices.Count;
            foreach (var surface in cityObject.surfaces)
			{             
                //findout if ity is already a triangle
                if (surface.outerRing.Count == 3 && surface.innerRings.Count == 0)
                {
                    List<int> newindices = new List<int> { count, count + 1, count + 2 };
                    count += 3;
                    indexlist.AddRange(newindices);
                    vertexlist.AddRange(surface.outerRing);
                    normallist.AddRange(defaultnormalList);
                     continue;
                }

                //Our mesh output data per surface
                Vector3[] surfaceVertices;
				Vector3[] surfaceNormals;
				Vector2[] surfaceUvs;
				int[] surfaceIndices;

                //offset using first outerRing vertex position
                Vector3Double offsetPolygons = surface.outerRing[0];
                List<Vector3> outside = new List<Vector3>();
                for (int i = 0; i < surface.outerRing.Count; i++)
                {
                    outside.Add((Vector3)(surface.outerRing[i] - offsetPolygons));
                }

                List<List<Vector3>> holes = new List<List<Vector3>>();
                for (int i = 0; i < surface.innerRings.Count; i++){
                    List<Vector3> inner = new List<Vector3>();
					for (int j = 0; j < surface.innerRings[i].Count; j++)
					{
                        inner.Add((Vector3)(surface.innerRings[i][j] - offsetPolygons));
                    }
                    holes.Add(inner);
				}

                

                //Turn poly into triangulated geometry data
                Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
				poly.outside = outside;
				poly.holes = holes;

                if (poly.outside.Count < 3)
                {
                    Console.WriteLine("Polygon seems to be a line");
                    continue;
                }
				Poly2Mesh.CreateMeshData(poly, out surfaceVertices,out surfaceNormals, out surfaceIndices, out surfaceUvs);

                var offset = subObject.vertices.Count;

                //Append verts, normals and uvs
                for (int j = 0; j < surfaceVertices.Length; j++)
				{
					subObject.vertices.Add(((Vector3Double)surfaceVertices[j]) + offsetPolygons);
                    subObject.normals.Add(surfaceNormals[j]);

                    if(surfaceUvs!= null)
					    subObject.uvs.Add(surfaceUvs[j]);
				}

                //Append indices ( corrected to offset )
				for (int j = 0; j < surfaceIndices.Length; j++)
				{
					subObject.triangleIndices.Add(offset + surfaceIndices[j]);
				}
			}

            if (vertexlist.Count>0)
            {
                subObject.vertices.AddRange(vertexlist);
                subObject.triangleIndices.AddRange(indexlist);
                subObject.normals.AddRange(normallist);
            }
        }

		public void Cancel()
		{
			throw new NotImplementedException();
		}
	}
}
