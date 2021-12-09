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
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using TileBakeLibrary;

namespace TileBakeTool
{
	class Program
	{
		private static string configFilePath = "";
		private static ConfigFile configFile;

		private static string sourcePath = "";
		private static string outputPath = "";
		private static string newline = "\n";

		private static string identifier = "id";
		private static string removeFromIdentifier = "";

		private static bool replaceExistingIDs = false;
		
		private static bool createBrotliCompressedFiles = false;
		private static bool createObjFiles = false;

		private static float lod = 0;
		private static string filterType = "";

		private static bool removeSpikes = false;
		private static float spikeCeiling = 0;
		private static float spikeFloor = 0;

		private static bool sliceGeometry = false;

		static void Main(string[] args)
		{
            //var test = new test();
            //test.readtest();



            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            if (args.Length == 0 || (args.Length == 1 && args[0].ToLower().Contains("help")))
            {
                //No parameters or an attempt to call for help? Show help in console.
                ShowHelp();
            }
            else if (args.Length == 1)
            {
                //One parameter? Assume its a config file path
                ApplyConfigFileSettings(args[0]);
            }
            else
            {
                //More parameters? Parse them
                ParseArguments(args);
            }

            //If we received the minimal settings to start, start converting!
            if (sourcePath != "" && outputPath != "")
                StartConverting(sourcePath, outputPath);
        }

        private static void DefaultArgument(string sourcePath)
		{
			if(!Path.IsPathFullyQualified(sourcePath)){
				Console.WriteLine($"Not a valid path: {sourcePath}");
				Console.WriteLine($"This might help: ");
				ShowHelp();
				return;
			}

			StartConverting(sourcePath, sourcePath);
		}

		private static void ParseArguments(string[] args)
		{
			//Read the arguments and apply corresponding settings
			for (int i = 0; i < args.Length; i++)
			{
				var argument = args[i];
				if (argument.Contains("--")){
					var value = (i + 1 < args.Length) ? args[i + 1] : "";
					ApplySetting(argument,value);
				}
			}
		}

		private static void ApplyConfigFileSettings(string configFilePath){
			if(File.Exists(configFilePath))
			{
				var configJsonText = File.ReadAllText(configFilePath);
				configFile = JsonSerializer.Deserialize<ConfigFile>(configJsonText
				, new JsonSerializerOptions()
				{ 
					AllowTrailingCommas = true }
				);

				sourcePath = configFile.sourceFolder;
				outputPath = configFile.outputFolder;

				removeSpikes = configFile.removeSpikes;
				spikeCeiling = configFile.removeSpikesAbove;
				spikeFloor = configFile.removeSpikesBelow;
				replaceExistingIDs = configFile.replaceExistingObjects;
				identifier = configFile.identifier;
				removeFromIdentifier = configFile.removePartOfIdentifier;
				if(configFile.lod != 0.0f) lod = configFile.lod;
				createBrotliCompressedFiles = configFile.brotliCompression;

				sliceGeometry = (configFile.tilingMethod == "SLICED"); //TILED or SLICED

				Console.WriteLine($"Loaded config file with settings");
			}
		}

		private static void ApplySetting(string argument, string value)
		{
			switch (argument)
			{
				case "--config":
					ApplyConfigFileSettings(value);
					break;
				case "--source":
					sourcePath = value;
					Console.WriteLine($"Source: {value}");
					break;
				case "--output":
					outputPath = value;
					Console.WriteLine($"Output directory: {value}");
					break;
				case "--replace":
					replaceExistingIDs = true;
					Console.WriteLine($"Replacing existing IDs");
					break;
				case "--lod":
					lod = float.Parse(value,System.Globalization.CultureInfo.InvariantCulture);
					Console.WriteLine($"LOD filter: {lod}");
					break;
				case "--type":
					filterType = value;
					Console.WriteLine($"Type filter: {filterType}");
					break;
				case "--id":
					identifier = value;
					Console.WriteLine($"Object identifier: {identifier}");
					break;
				case "--id-remove":
					removeFromIdentifier = value;
					Console.WriteLine($"Remove from identifier: {removeFromIdentifier}");
					break;
				case "--brotli":
					createBrotliCompressedFiles = true;
					break;
				case "--obj":
					createObjFiles = true;
					break;
				default:
					
					break;
			}
		}

		private static void StartConverting(string sourcePath, string targetPath)
		{
			Console.WriteLine("Starting...");

			//Here we use the .dll. This usage is the same as in Unity3D.
			var tileBaker = new CityJSONToTileConverter();
			tileBaker.SetSourcePath(sourcePath);
			tileBaker.SetTargetPath(targetPath);
			tileBaker.SetLOD(lod);
			tileBaker.SetFilterType(filterType);
			tileBaker.SetID(identifier, removeFromIdentifier);
			tileBaker.SetReplace(replaceExistingIDs);
			tileBaker.CreateOBJ(createObjFiles);
			tileBaker.AddBrotliCompressedFile(createBrotliCompressedFiles);
			
			if (configFile != null)
			{
				tileBaker.SetClipSpikes(removeSpikes, spikeCeiling, spikeFloor);
				tileBaker.SetObjectFilters(configFile.cityObjectFilters);
			}
			tileBaker.TilingMethod = configFile.tilingMethod;

			tileBaker.Convert();
		}

		private static void ShowHelp()
		{
			Console.Write(Constants.helpText);
		}
	}
}
