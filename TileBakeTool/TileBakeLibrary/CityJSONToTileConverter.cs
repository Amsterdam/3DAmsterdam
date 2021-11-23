using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using TileBakeLibrary.Coordinates;
using Bunny83.SimpleJSON;
using System.Linq;

namespace TileBakeLibrary
{
	public class CityJSONToTileConverter : IConverter
	{
		private string sourcePath = "";
		private string outputPath = "";

        private string identifier = "";
        private string removeFromID = "";

        private bool addToExistingTiles = false;

        private float lod = 0;
        private int tileSize = 1000;

        private List<SubObject> allCityObjects = new List<SubObject>();
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
                allCityObjects.AddRange(task.Result);
            }
            
            if(addToExistingTiles)
            {
                ParseExisistingTiles();
			}

            BakeTiles();
		}


		private void ParseExisistingTiles()
        {
            
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
            foreach (SubObject cityObject in allCityObjects)
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
                    tiles.Add(new Tile()
                    {
                        position = new Vector2Double(tileX, tileY),
                    });
                }
            }

            Console.WriteLine($"Baking {XTiles}x{YTiles} = {XTiles*YTiles} tiles");

            //Add the CityObjects that fall within the bounds
            //TODO<----

            //Create binary files
            Directory.CreateDirectory(outputPath);
            foreach (Tile tile in tiles) {
                //Create binary files
                BinaryMeshWriter.SaveAsBinaryFile(tile, $"{outputPath}{tile.position.X}_{tile.position.Y}.bin");
            }
        }

		private async Task<List<SubObject>> AsyncParseProcess(string sourceFile)
		{
            List<SubObject> foundCityObjects = new List<SubObject>();

            //Parse the file
            Console.WriteLine($"Parsing CityJSON: {sourceFile}");
            var jsonstring = File.ReadAllText(sourceFile);
            await Task.Delay(10);

            var cityjsonNode = JSON.Parse(jsonstring);
            if (cityjsonNode == null || cityjsonNode["CityObjects"] == null)
            {
                Console.WriteLine($"Failed to parse CityJSON file {sourceFile}");
                return null;
            }

            //Get vertices
            var allVerts = new List<Vector3Double>();

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
                var vert = new Vector3Double(
                       node[0].AsDouble * transformScale.X + transformOffset.X,
                       node[1].AsDouble * transformScale.Y + transformOffset.Y,
                       node[2].AsDouble * transformScale.Z + transformOffset.Z
                );

                allVerts.Add(vert);
            }

            //Build the meshes and create the city objects
            foreach (JSONNode buildingNode in cityjsonNode["CityObjects"])
            {
                //Object ID
                var objectID = "";
                if (buildingNode["attributes"][identifier] != null) {
                    objectID = buildingNode["attributes"][identifier].Value;
                    if(objectID.Length > 0 && removeFromID.Length > 0)
                    {
                        objectID = objectID.Replace(removeFromID, "");
                    }
                }

                var geometries = buildingNode["geometry"].AsArray;
                foreach (JSONNode geometry in geometries)
                {
                    if (geometry["lod"].AsFloat == lod && geometry["type"] == "Solid")
                    {
                        int[] indices = geometry["boundaries"][0][0][0].Children.Select(n => n.AsInt).ToArray();

                        //For testing just interpret first ones as triangle
                        List<Vector3Double> triangle = new List<Vector3Double>();
                        triangle.Add(allVerts[indices[0]]);
                        triangle.Add(allVerts[indices[1]]);
                        triangle.Add(allVerts[indices[2]]);

                        List <Vector3Double> thisMeshVerts = new List<Vector3Double>();
                        var newCityObject = new SubObject()
                        {
                            id = objectID,
                            verticesRD = triangle,
                            centroid = triangle[0]
                        };
                        foundCityObjects.Add(newCityObject);
                    }
                }
            }
            return foundCityObjects;
        }

		public void Cancel()
		{
			throw new NotImplementedException();
		}
	}
}
