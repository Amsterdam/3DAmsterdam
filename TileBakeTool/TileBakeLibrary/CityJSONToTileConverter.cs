﻿using System;
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
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            List<SubObject> filteredObjects = new List<SubObject>();

            Console.WriteLine($"Parsing CityJSON: {sourceFile}");
            await Task.Delay(10);

            var cityJson = new CityJSON(sourceFile, true, true);
            List<CityObject> cityObjects = cityJson.LoadCityObjects(lod);
            Console.WriteLine($"CityObjects found: {cityObjects.Count}");
            for (int i = 0; i < cityObjects.Count; i++)
			{
                var cityObject = cityObjects[i];
                var subObject = ToSubObject(cityObject);
                if (subObject != null)
                    filteredObjects.Add(subObject);
            }

            return filteredObjects;
        }

		private SubObject ToSubObject(CityObject cityObject)
		{
            var subObject = new SubObject();
            subObject.verticesRD = new List<Vector3Double>();
            subObject.normals = new List<Vector3>();
            subObject.uvs = new List<Vector2>();
            subObject.triangleIndices = new List<int>();

            //Find the identifier in attributes semantic or parent attributes
            //If there is no ID, it might be a child. And we can skip it.
            var id = "";
            foreach (var semantic in cityObject.semantics)
            {
                if (semantic.value == identifier)
                {
                    id = semantic.value;
                    break;
                }
            }
            if (id == "") return null;

			for (int i = 0; i < cityObject.children.Count; i++)
			{
				var childObject = cityObject.children[i];
				//Add child geometry to our subobject
				AppendCityObjectGeometry(childObject, subObject);
			}

			return subObject;
        }

		private static void AppendCityObjectGeometry(CityObject childObject, SubObject subObject)
		{
			foreach (var surface in childObject.surfaces)
			{
				//Our mesh output data per surface
				Vector3[] surfaceVertices;
				Vector2[] surfaceUvs;
				int[] surfaceIndices;

				//TODO make poly2mesh have double precision so we only loose precision at bake time
				Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
				poly.outside = surface.outerRing.Cast<Vector3>().ToList();
				poly.holes = surface.outerRing.Cast<List<Vector3>>().ToList();
				Poly2Mesh.CreateMeshData(poly, out surfaceVertices, out surfaceIndices, out surfaceUvs);
				for (int j = 0; j < surfaceVertices.Length; j++)
				{
					subObject.verticesRD.Add((Vector3Double)surfaceVertices[j]);
					subObject.uvs.Add(surfaceUvs[j]);
				}
				for (int j = 0; j < surfaceIndices.Length; j++)
				{
					subObject.triangleIndices.Add(subObject.verticesRD.Count + surfaceIndices[j]);
				}
			}
		}

		public void Cancel()
		{
			throw new NotImplementedException();
		}
	}
}
