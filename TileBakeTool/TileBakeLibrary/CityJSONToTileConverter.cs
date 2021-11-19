using System;
using System.IO;
using System.Numerics;
using TileBakeLibrary.Coordinates;

namespace TileBakeLibrary
{
	public class CityJSONToTileConverter : IConverter
	{
		private string sourcePath = "";
		private string targetPath = "";

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
			targetPath = target;
		}

		/// <summary>
		/// Start converting the .cityjson files into binary tile files
		/// </summary>
		public void Convert()
		{
			var someDoubleVector3 = new Vector3Double(
				1,
				2,
				3
			);

			File.WriteAllText($"{targetPath}/dummy.txt", "dummy file " + someDoubleVector3);
		}

		/// <summary>
		/// Cancels the running concersion progress
		/// </summary>
		public void Cancel()
		{
			
		}
	}
}
