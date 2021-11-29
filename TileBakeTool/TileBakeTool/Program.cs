using System;
using System.IO;
using System.Reflection;
using System.Threading;
using TileBakeLibrary;

namespace TileBakeTool
{
	class Program
	{
		private static string sourcePath = "";
		private static string outputPath = "";
		private static string newline = "\n";

		private static string identifier = "id";
		private static string removeFromIdentifier = "";

		private static bool addToExistingTiles = false;
		
		private static bool createBrotliCompressedFiles = false;
		private static bool createObjFiles = false;

		private static float lod = 0;
		private static string filterType = "";

		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

			if (args.Length == 0 || (args.Length == 1 && args[0].ToLower().Contains("help")))
			{
				//No parameters or an attempt to call for help? Show help in console.
				ShowHelp();
			}
			else if (args.Length == 1)
			{
				//One parameter? Assume its a source path.
				DefaultArgument(args[0]);
			}
			else
			{
				//More parameters? Parse them
				ParseArguments(args);
			}
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

			if(sourcePath != "" && outputPath != "")
				StartConverting(sourcePath, outputPath);
		}

		private static void ApplySetting(string argument, string value)
		{
			switch (argument)
			{
				case "--source":
					sourcePath = value;
					Console.WriteLine($"Source: {value}");
					break;
				case "--output":
					outputPath = value;
					Console.WriteLine($"Output directory: {value}");
					break;
				case "--add":
					addToExistingTiles = true;
					Console.WriteLine($"Output directory: {value}");
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
			tileBaker.SetAdd(addToExistingTiles);
			tileBaker.CreateOBJ(createObjFiles);
			tileBaker.AddBrotliCompressedFile(createBrotliCompressedFiles);
			tileBaker.Convert();
		}

		private static void ShowHelp()
		{
			Console.Write(Constants.helpText);
		}
	}
}
