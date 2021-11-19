using System;
using System.IO;
using System.Reflection;
using TileBakeLibrary;

namespace TileBakeTool
{
	class Program
	{
		static void Main(string[] args)
		{
			

			ParseArguments(args);

			var sourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var targetPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var tileBaker = new CityJSONToTileConverter();
			tileBaker.SetSourcePath(sourcePath);
			tileBaker.SetTargetPath(targetPath);

			tileBaker.Convert();
		}

		private static void ParseArguments(string[] args)
		{
			foreach(var arg in args)
				Console.Write(arg);
		}
	}
}
