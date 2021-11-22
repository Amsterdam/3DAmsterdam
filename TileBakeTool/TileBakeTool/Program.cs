using System;
using System.IO;
using System.Reflection;
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


		private static int lodIndex = 0;

		static void Main(string[] args)
		{
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

		private static void ShowHelp()
		{
			Console.Write(@"
This tool converts CityJSON files into Netherlands3D binary tile files.
Check out http:/3d.amsterdam.nl/netherlands3d for help.
Required options
--source <pathToSourceFolder>
--output <pathToTileOutputFolder>

Extra options:

--add						Add objects to existing binary tile files
--replace					Replace objects with the same ID
--id <property name>		Unique ID property name
--id-remove <string>		Remove this substring from the ID's
--filter-type <type>		Filter object on type
--filter-lod <lod>			Target LOD index

Pipeline example:

TileBakeTool.exe --source ""C:/MyProject/CityJsonFiles/*.json"" --output ""C:/MyProject/BinaryTiles/"" --filter-lod ""2"" --filter-type ""gebouw"" --id ""GebouwNummer""

TileBakeTool.exe --source ""C:/MyProject/CustomMadeBuildings/*.json""--output ""C:/MyProject/BinaryTiles/"" --id ""BAGID"" --add --replace");
		}

		private static void ParseArguments(string[] args)
		{
			//Read the arguments and apply corresponding settings
			for (int i = 0; i < args.Length; i++)
			{
				var argument = args[i];
				if (argument.Contains("--")){
					var value = (i + 1 < args.Length) ? "" : args[i + 1];
					ApplySetting(argument,value);
				}
			}

			if(sourcePath != "" && outputPath != "")
				StartConverting(sourcePath, outputPath);
		}

		private static void StartConverting(string sourcePath, string targetPath)
		{
			var tileBaker = new CityJSONToTileConverter();
			tileBaker.SetSourcePath(sourcePath);
			tileBaker.SetTargetPath(targetPath);
			tileBaker.SetLODSlot(lodIndex);
			tileBaker.SetID(identifier, removeFromIdentifier);
			tileBaker.Convert();
		}

		private static void ApplySetting(string argument, string value)
		{
			Console.WriteLine($"Setting {argument} = {value}");
			switch (argument)
			{
				case "--source":
					sourcePath = value;
					break;
				case "--output":
					outputPath = value;
					break;
				case "--filter-lod":
					lodIndex = int.Parse(value);
					break;
				case "--id":
					identifier = value;
					break;
				case "--id-remove":
					removeFromIdentifier = value;
					break;
				default:
					break;
			}
		}
	}
}
