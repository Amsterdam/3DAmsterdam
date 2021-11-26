using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBakeLibrary
{
	class BrotliCompress
	{
		public static void Compress(string input, string output = "")
		{
			if(!File.Exists(input))
			{
				throw new FileNotFoundException();
			}

			//Read bytes into memory
			byte[] bytes = File.ReadAllBytes(input);
			long originalSize = bytes.Length; 

			//Compress to brotli
			using (var outputStream = new MemoryStream())
			{
				using (var compressStream = new BrotliStream(outputStream, CompressionLevel.Optimal))
				{
					compressStream.Write(bytes, 0, bytes.Length);
				}
				if (output == "")
				{
					output = input + ".br";
				}

				//Show the compression results in our console
				byte[] compressedBytes = outputStream.ToArray();
				long compressedSize = compressedBytes.Length;
				float percentage = ((float)compressedSize / (float)originalSize) * 100.0f;
				Console.WriteLine($"Brotli compressed: {BytesToString(originalSize)} -> {BytesToString(compressedSize)} = ({percentage:F2}%)");

				File.WriteAllBytes(output, compressedBytes);
			}
		}

		static string BytesToString(long byteCount)
		{
			string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
			if (byteCount == 0)
				return "0" + suf[0];
			long bytes = Math.Abs(byteCount);
			int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
			double num = Math.Round(bytes / Math.Pow(1024, place), 1);
			return (Math.Sign(byteCount) * num).ToString() + suf[place];
		}
	}
}
