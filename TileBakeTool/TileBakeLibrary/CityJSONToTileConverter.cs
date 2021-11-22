using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using TileBakeLibrary.Coordinates;
using SimpleJSON;
using System.Linq;

namespace TileBakeLibrary
{
	public class CityJSONToTileConverter : IConverter
	{
		private string sourcePath = "";
		private string outputPath = "";

        private string identifier = "";
        private string removeFromID = "";

        private int lodIndex = 0;

        private List<CityObject> allCityObjects = new List<CityObject>();
        private List<Tile> tiles = new List<Tile>();
        private Task<List<CityObject>>[] conversionTasks;

        /// <summary>
        /// The LOD index we want to parse. 
        /// </summary>
        /// <param name="targetLodIndex">Defaults to 0</param>
        public void SetLODSlot(int targetLodIndex)
        {
            lodIndex = targetLodIndex;
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
			conversionTasks = new Task<List<CityObject>>[sourceFiles.Length];
			for (int i = 0; i < sourceFiles.Length; i++)
			{
				conversionTasks[i] = Task.Run(() => ConvertProcess(sourceFiles[i]));
			}

            //Wait for all the files to be parsed, and combine the results
			Task.WaitAll(conversionTasks);
            foreach (var task in conversionTasks)
            {
                allCityObjects.AddRange(task.Result);
            }

            BakeTiles();
			Console.ReadLine();
		}

        /// <summary>
        /// Bake the city objects into binary tiles
        /// </summary>
        private void BakeTiles()
        {
            File.WriteAllLines(outputPath, allCityObjects.Select(cityObject => cityObject.id).ToArray());
		}

		private async Task<List<CityObject>> ConvertProcess(string sourceFile)
		{
            List<CityObject> foundCityObjects = new List<CityObject>();

            //Parse the file
            var jsonstring = File.ReadAllText(sourceFile);
            Console.WriteLine($"Parsing CityJSON: {jsonstring}");
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
                       node[0].AsDouble * transformScale.x + transformOffset.x,
                       node[1].AsDouble * transformScale.y + transformOffset.y,
                       node[2].AsDouble * transformScale.z + transformOffset.z
                );

                allVerts.Add(vert);
            }

            //Now build the meshes and create the city objects
            foreach (JSONNode buildingNode in cityjsonNode["CityObjects"])
            {
                //Object ID
                var objectID = buildingNode["attributes"][identifier].Value.Replace(removeFromID, "");
                                
                //The building verts/triangles
                var boundaries = buildingNode["geometry"][lodIndex]["boundaries"][0];

                var newCityObject = new CityObject()
                {
                    id = objectID
				};

                foundCityObjects.Add(newCityObject);
            }
            return foundCityObjects;
        }

        /// <summary>
        /// Add our parsed city object
        /// </summary>
        /// <param name="id"></param>
        private void AddCityObject(string id)
        {
            allCityObjects.Add(
                new CityObject()
                {
                    id = id
                }
            );
		}

		/// <summary>
		/// Cancels the running concersion progress
		/// </summary>
		public void Cancel()
		{
            
        }
	}
}
