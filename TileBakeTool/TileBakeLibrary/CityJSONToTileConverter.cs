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

namespace TileBakeLibrary
{
	public class CityJSONToTileConverter
	{
		private string sourcePath = "";
		private string outputPath = "";

        private string identifier = "";
        private string removeFromID = "";

        private bool parseExistingTiles = false;

        private float mergeVerticesBelowNormalAngle = 0.0f; 

        private bool addToExistingTiles = false;

        private float lod = 0;
        private int tileSize = 1000;

        private List<SubObject> allSubObjects = new List<SubObject>();
        private List<Tile> tiles = new List<Tile>();
        private Task<List<SubObject>>[] parseTasks;
		

		/// <summary>
		/// The LOD we want to parse. 
		/// </summary>
		/// <param name="targetLOD">Defaults to 0</param>
		public void SetLOD(float targetLOD)
        {
            lod = targetLOD;
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
		public void SetAdd(bool add)
		{
            this.addToExistingTiles = add;
        }

		/// <summary>
		/// Start converting the cityjson files into binary tile files
		/// </summary>
		public void Convert()
		{
            //Make sure 

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

            parseTasks = new Task<List<SubObject>>[sourceFiles.Length];
			for (int i = 0; i < sourceFiles.Length; i++)
			{
                var index = i;
                Task<List<SubObject>> newParseTask = Task.Run(() => AsyncParseProcess(sourceFiles[index]));
                parseTasks[i] = newParseTask;
			}

            //Wait for all the files to be parsed, and combine the results
			Task.WaitAll(parseTasks);
            foreach (var task in parseTasks)
            {
                allSubObjects.AddRange(task.Result);
            }
            
            if(addToExistingTiles)
            {
                ParseExisistingTiles();
			}

            BakeTiles();
		}

		private void ParseExisistingTiles()
		{
			throw new NotImplementedException();
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
                Console.WriteLine($"CityObject: {cityObject.id}");

                if (cityObject.centroid.X < minX) minX = cityObject.centroid.X;
                else if (cityObject.centroid.X >= maxX) maxX = cityObject.centroid.X;

                if (cityObject.centroid.Y < minY) minY = cityObject.centroid.Y;
                else if (cityObject.centroid.Y >= maxY) maxY = cityObject.centroid.Y;
            }

            //Create our grid of tiles
            var XTiles = Math.Ceiling((maxX - minX) / tileSize);
            var YTiles = Math.Ceiling((maxY - minY) / tileSize);

            var startXRD = Math.Floor(minX / tileSize) * tileSize;
            var startYRD = Math.Floor(minY / tileSize) * tileSize;

            for (int x = 0; x < XTiles; x++)
			{
                var tileX = startXRD + (x * tileSize);
                for (int y = 0; y < YTiles; y++)
                {
                    var tileY = startYRD + (y * tileSize);
                    var newTile = new Tile()
                    {
                        position = new Vector2Double(tileX, tileY),
                        filePath = $"{outputPath}{tileX}_{tileY}.bin"
                    };
                    tiles.Add(newTile);

                    //Parse exisiting file
                    if(parseExistingTiles && File.Exists(newTile.filePath))
                    {
                        ParseExistingBinaryTile(newTile);
                    }
                }
            }
            
            Console.WriteLine($"Baking {XTiles}x{YTiles} = {XTiles*YTiles} tiles");

			//Move the CityObjects into to the proper tile based on their centroid
			for (int i = allSubObjects.Count - 1; i >= 0; i--)
			{
                var subObject = allSubObjects[i];
                foreach(var tile in tiles)
                {
                    if (subObject.centroid.X > tile.position.X
                    && subObject.centroid.X <= tile.position.X + tileSize
                    && subObject.centroid.Y > tile.position.Y
                    && subObject.centroid.Y <= tile.position.Y + tileSize)
                    {
                        tile.AddSubObject(subObject);
                        allSubObjects.Remove(subObject);
                    };
				}
            }

            //Create binary files
            Directory.CreateDirectory(outputPath);
            foreach (Tile tile in tiles) {
                Console.WriteLine($"Saving {tile.filePath} containing {tile.SubObjects.Count} SubObjects");

                //Create binary files
                BinaryMeshWriter.SaveAsBinaryFile(tile);
            }
        }

        /// <summary>
        /// Read SubObjects from existing binary tile file
        /// </summary>
		private void ParseExistingBinaryTile(Tile tile)
		{
            BinaryMeshReader.ReadBinaryFile(tile);
		}

		private async Task<List<SubObject>> AsyncParseProcess(string sourceFile)
		{
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            List<SubObject> filteredObjects = new List<SubObject>();

            Console.WriteLine($"Parsing CityJSON: {sourceFile}");
            await Task.Delay(10);

            var cityJson = new CityJSON(sourceFile, true, true);
            List<CityObject> cityObjects = cityJson.LoadCityObjects(lod);
            Console.WriteLine($"CityObjects found: {cityObjects.Count}");

            //Turn cityobjects (and their children) into SubObject mesh data
            for (int i = 0; i < cityObjects.Count; i++)
            {
                var cityObject = cityObjects[i];
                var subObject = ToSubObjectMeshData(cityObject);
                if (subObject != null)
                {
                    filteredObjects.Add(subObject);
                }
            }

            return filteredObjects;
        }

		private SubObject ToSubObjectMeshData(CityObject cityObject)
		{
            var subObject = new SubObject();
            subObject.vertices = new List<Vector3Double>();
            subObject.normals = new List<Vector3>();
            subObject.uvs = new List<Vector2>();
            subObject.triangleIndices = new List<int>();

            //Find the identifier in attributes semantic or parent attributes
            //If there is no ID, it might be a child. And we can skip it.
            var id = "";
            foreach (var semantic in cityObject.semantics)
            {
                Console.WriteLine(semantic.name);
                if (semantic.name == identifier)
                {
                    id = semantic.value;
                    break;
                }
            }

            AppendCityObjectGeometry(cityObject, subObject);

            //Append all child geometry
            for (int i = 0; i < cityObject.children.Count; i++)
			{
				var childObject = cityObject.children[i];
				//Add child geometry to our subobject. (Recursive disabled, so only one child level is allowed)
				AppendCityObjectGeometry(childObject, subObject, false);
			}
            if (mergeVerticesBelowNormalAngle != 0)
            {
                subObject.MergeSimilarVertices(mergeVerticesBelowNormalAngle);
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

		private static void AppendCityObjectGeometry(CityObject childObject, SubObject subObject, bool recursive = false)
		{
			foreach (var surface in childObject.surfaces)
			{
				//Our mesh output data per surface
				Vector3[] surfaceVertices;
				Vector3[] surfaceNormals;
				Vector2[] surfaceUvs;
				int[] surfaceIndices;

                //TODO make poly2mesh have double precision so we only loose precision at bake time, and not having to convert these lists
                List<Vector3> outside = new();
                for (int i = 0; i < surface.outerRing.Count; i++)
                {
                    outside.Add((Vector3)surface.outerRing[i]);
                }
                List<List<Vector3>> holes = new();
                for (int i = 0; i < surface.innerRings.Count; i++){
                    List<Vector3> inner = new();
					for (int j = 0; j < surface.innerRings[i].Count; j++)
					{
                        inner.Add((Vector3)surface.innerRings[i][j]);
                    }
                    holes.Add(inner);
				}

                //Turn poly into triangulated geometry data
                Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
				poly.outside = outside;
				poly.holes = holes;
				Poly2Mesh.CreateMeshData(poly, out surfaceVertices,out surfaceNormals, out surfaceIndices, out surfaceUvs);
                for (int j = 0; j < surfaceVertices.Length; j++)
				{
					subObject.vertices.Add((Vector3Double)surfaceVertices[j]);
                    subObject.normals.Add(surfaceNormals[j]);
                    if (surfaceUvs != null)
					    subObject.uvs.Add(surfaceUvs[j]);
				}
				for (int j = 0; j < surfaceIndices.Length; j++)
				{
					subObject.triangleIndices.Add(subObject.vertices.Count + surfaceIndices[j]);
				}
			}

            if (recursive)
            {
                for (int i = 0; i < childObject.children.Count; i++)
                {
                    var revursiveChildObject = childObject.children[i];
                    AppendCityObjectGeometry(revursiveChildObject, subObject, recursive);
                }
            }
        }

		public void Cancel()
		{
			throw new NotImplementedException();
		}
	}
}
