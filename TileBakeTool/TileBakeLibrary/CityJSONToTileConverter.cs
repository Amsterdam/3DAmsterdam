/*
*  Copyright (C) X Gemeente
*              	 X Amsterdam
*				 X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://joinup.ec.europa.eu/software/page/eupl
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
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
using TileBakeLibrary.BinaryMesh;
using System.Diagnostics;

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

        private bool clipSpikes = false;
        private float spikeCeiling = 0;
        private float spikeFloor = 0;

        public void setClipSpikes(bool setFunction,float ceiling,float floor)
        {
            clipSpikes = setFunction;
            spikeCeiling = ceiling;
            spikeFloor = floor;
        }

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

        private int filecounter = 0;
        private int totalFiles = 0;
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
            totalFiles = sourceFiles.Length;
            if (sourceFiles.Length>0)
            {
                cityJson = new CityJSON(sourceFiles[0], true, true);
            }
            

            for (int i = 0; i < sourceFiles.Length; i++)
			{
                CityJSON nextCityJSON=null;
                int nextJsonID = i + 1;
                if (i+1==sourceFiles.Length)
                {
                    nextJsonID = i;
                }
                
                Thread thread;
                thread = new Thread(
                       () => {
                           nextCityJSON = new CityJSON(sourceFiles[nextJsonID], true, true);
                       }
                       );


                    thread.Start();


                Stopwatch watch = new Stopwatch();
                watch.Start();
                tiles = new List<Tile>();
                var index = i;
                filecounter++;
                var cityObjects = CityJSONParseProcess(cityJson);
                allSubObjects.Clear();
                allSubObjects=cityObjects;
                Console.WriteLine($"\r{allSubObjects.Count} CityObjects imported                                                                                    ");
                PrepareTiles();
                WriteTileData();
                allSubObjects.Clear();
                cityJson = null;
                GC.Collect();
                watch.Stop();
                var result = watch.Elapsed;
                string elapsedTimeString = string.Format("{0}:{1} minutes",
                                          result.Minutes.ToString("00"),
                                          result.Seconds.ToString("00"));
                Console.WriteLine($"duration: {elapsedTimeString}");

                    thread.Join();
                    cityJson = nextCityJSON;

                }
            if (brotliCompress) CompressFiles();
        }



        private void PrepareTiles()
        {
            TileSubobjects();
            AddObjectsFromBinaryTile();
            

        }

        private void TileSubobjects()
        {
            tiles.Clear();
            foreach (SubObject cityObject in allSubObjects)
            {
                double tileX = Math.Floor(cityObject.centroid.X / tileSize) * (int)tileSize;
                double tileY = Math.Floor(cityObject.centroid.Y / tileSize) * (int)tileSize;
                if (tileX==0 || tileY==0)
                {
                    Console.WriteLine("found cityObject with no geometry");
                }
                Vector2Double tileposition;
                bool found = false;
                for (int i = 0; i < tiles.Count; i++)
                {
                    tileposition = tiles[i].position;
                    if (tileposition.X ==tileX)
                    {
                        if (tileposition.Y==tileY)
                        {
                            tiles[i].SubObjects.Add(cityObject);
                            found = true;
                            break;
                        }

                    }
                }
                if (!found)
                {
                    Tile newtile = new Tile();
                    newtile.size = new Vector2(tileSize, tileSize);
                    newtile.position = new Vector2Double(tileX, tileY);
                    newtile.filePath = $"{outputPath}{tileX}_{tileY}.{lod}.bin";
                    newtile.SubObjects.Add(cityObject);
                    tiles.Add(newtile);
                }
            }
        }

        private void AddObjectsFromBinaryTile()
        {
            foreach (Tile tile in tiles)
            {
                //Parse exisiting file
                if (File.Exists(tile.filePath))
                { 
                    ParseExistingBinaryTile(tile);
                }
            }
        }

        private void WriteTileData()
        {
            //Create binary files (if we added subobjects to it)
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            Console.Write($"\rBaking {tiles.Count} tiles");
            //Threaded writing of binary meshes + compression
            Console.Write("\rsaving files...          ");
            int counter = 0;

            Parallel.ForEach(tiles, tile =>
            {
                if (tile.SubObjects.Count == 0)
                {
                    //Console.WriteLine($"Skipping {tile.filePath} containing {tile.SubObjects.Count} SubObjects");
                }
                else
                {
                    //Bake the tile, and lets save it!

                    //tile.Bake();

                    //Console.WriteLine($"Saving {tile.filePath} containing {tile.SubObjects.Count} SubObjects");
                    
                    //Create binary files
                    //BinaryMeshWriter.Save(tile);
                    BinaryMeshData bmd = new BinaryMeshData();
                    bmd.ExportData(tile);

                    //Compressed variant
                    //if (brotliCompress) BrotliCompress.Compress(tile.filePath);

                    //Optionaly write other format(s) for previewing purposes
                    if (createOBJFiles) OBJWriter.Save(tile);
                    Interlocked.Increment(ref counter);
                    Console.Write($"\rsaving files...{counter}");
                }
            });

            Console.WriteLine($"\r{counter} files saved                                                                                             ");
        }

        public void CompressFiles()
        {
           
            var filter = $"*{lod}.bin";

            //List the files that we are going to parse
            string[] binFiles = Directory.GetFiles(Path.GetDirectoryName(outputPath), filter);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int counter = 0;
            int totalcount = binFiles.Length;
            Console.Write("\rcompressing files");
            Parallel.ForEach(binFiles, filename =>
            {
                if (brotliCompress) BrotliCompress.Compress(filename);
                Interlocked.Increment(ref counter);
                Console.Write($"\r compressing files {counter} of {totalcount}");

            });
            watch.Stop();
            var result = watch.Elapsed;
            string elapsedTimeString = string.Format("{0}:{1} minutes",
                                      result.Minutes.ToString("00"),
                                      result.Seconds.ToString("00"));
            Console.WriteLine($"duration: {elapsedTimeString}");
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
            BinaryMeshData bmd = new BinaryMeshData();
            bmd.importData(tile);
            bmd = null;
            
            // Console.WriteLine($"Parsed existing tile {tile.filePath} with {tile.SubObjects.Count} subobjects");
        }


        private List<SubObject> CityJSONParseProcess(CityJSON cityJson)
        {
            List<SubObject> filteredObjects = new List<SubObject>();
            Console.WriteLine("");
           // Console.WriteLine($"Parsing CityJSON {filecounter} of {totalFiles}: {sourceFile}");
           // Console.Write("loading file...");
            
            
            Console.Write("\r reading cityobjects");
            //List<CityObject> cityObjects = cityJson.LoadCityObjects(lod);
            int cityObjectCount = cityJson.CityObjectCount();
            Console.WriteLine($"\r CityObjects found: {cityObjectCount}");
            Console.Write("---");
            int skipped = 0;
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
                 Console.Write("\r" + done + " done; " + skipped + " skipped ; " + parsing + " parsing; " + simplifying + " simplifying; " + tiling + " tiling                    ");
                 var subObject = ToSubObjectMeshData(cityObject);
                 cityJson.ClearCityObject(cityObject.keyName);
                 cityObject = null;
                 Interlocked.Decrement(ref parsing);
                 
                 cityObject = null;
                 if (subObject == null)
                 {
                     Interlocked.Increment(ref done);
                     Interlocked.Increment(ref skipped);
                     return;
                 }
                 Console.Write("\r" + done + " done; " + skipped + " skipped ; " + parsing + " parsing; " + simplifying + " simplifying; " + tiling + " tiling                    ");

                 if (subObject.maxVerticesPerSquareMeter > 0)
                 {
                     Interlocked.Increment(ref simplifying);
                     Console.Write("\r" + done + " done; " + skipped + " skipped ; " + parsing + " parsing; " + simplifying + " simplifying; " + tiling + " tiling                    ");
                     subObject.SimplifyMesh();
                     Interlocked.Decrement(ref simplifying);
                     Console.Write("\r" + done + " done; " + skipped + " skipped ; " + parsing + " parsing; " + simplifying + " simplifying; " + tiling + " tiling                    ");
                 }
                 else
                 {
                     if (maxNormalAngle != 0)
                     {
                         subObject.MergeSimilarVertices(maxNormalAngle);
                     }
                 }
                 if (clipSpikes)
                 {
                     subObject.ClipSpikes(spikeCeiling,spikeFloor);
                 }

                 if (TilingMethod == "TILED")
                 {
                     Interlocked.Increment(ref tiling);
                     Console.Write("\r" + done + " done; " + skipped + " skipped ; " + parsing + " parsing; " + simplifying + " simplifying; " + tiling + " tiling                    ");
                     var newSubobjects = subObject.clipSubobject(new Vector2(tileSize, tileSize));
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
                     Console.Write("\r" + done + " done; " + skipped + " skipped ; " + parsing + " parsing; " + simplifying + " simplifying; " + tiling + " tiling                    ");
                 }
                 else
                 {
                     filterobjectsBucket.Add(subObject);
                 }

                 Interlocked.Increment(ref done);
                 Console.Write("\r" + done + " done; " + skipped + " skipped ; " + parsing + " parsing; " + simplifying + " simplifying; " + tiling + " tiling                    ");


             }
            );

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
                return null;
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
            bool calculateNormals = false;
            if (subObject.maxVerticesPerSquareMeter==0)
            {
                calculateNormals = true;
            }
            AppendCityObjectGeometry(cityObject, subObject,calculateNormals);
            //Append all child geometry too
            for (int i = 0; i < cityObject.children.Count; i++)
			{
				var childObject = cityObject.children[i];
				//Add child geometry to our subobject. (Recursive children are not allowed in CityJson)
				AppendCityObjectGeometry(childObject, subObject, calculateNormals);
			}

            //Winding order of triangles should be reversed
            subObject.triangleIndices.Reverse();

            //Check if the list if triangles is complete (divisible by 3)
            if (subObject.triangleIndices.Count % 3 != 0)
            {
                Console.WriteLine($"{subObject.id} triangle list is not divisible by 3. This is not correct.");
                return null;
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

		private static void AppendCityObjectGeometry(CityObject cityObject, SubObject subObject, bool calculateNormals)
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
                    if (calculateNormals)
                    {
                        Vector3 normal = calculateNormal(surface.outerRing[0], surface.outerRing[1], surface.outerRing[2]);
                        normallist.Add(normal);
                        normallist.Add(normal);
                        normallist.Add(normal);
                    }
                    else
                    {
                        normallist.AddRange(defaultnormalList);
                    }
                    
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
                //Poly2Mesh takes care of calculating normals, using a right-handed coordinate system
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
        private static Vector3 calculateNormal(Vector3Double v1, Vector3Double v2, Vector3Double v3)
        {
            Vector3 normal = new Vector3();
            Vector3Double U = v2 - v1;
            Vector3Double V = v3 - v1;

            double X = ((U.Y * V.Z) - (U.Z * V.Y));
            double Y = ((U.Z * V.X) - (U.X * V.Z));
            double Z = ((U.X * V.Y) - (U.Y * V.X));

            // normalize it
            double scalefactor = Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
            normal.X = (float)(X / scalefactor);
            normal.Y = (float)(Y / scalefactor);
            normal.Z = (float)(Z / scalefactor);
            return normal;

        }
		public void Cancel()
		{
			throw new NotImplementedException();
		}
	}
}
