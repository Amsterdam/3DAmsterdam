using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace TextureCombiner
{

	public class TexturedMesh
	{
		public string TextureName;
		public Mesh mesh;
		public Texture2D texture;
		public string IdentifierString;

		public TexturedMesh()
		{
			TextureName = "";
			mesh = null;
			texture = null;
		}
	}
	public class CombineTextures : MonoBehaviour
	{
		public static List<TexturedMesh> MultiCombineTextures(List<TexturedMesh> Texturedmeshes)
		{
			List<TexturedMesh> output = new List<TexturedMesh>();
			int maxwidth = 2048;
			int maxheight = 2048;

			List<TextureBlock> TextureBlocks = new List<TextureBlock>();
			// add meshes to textureblocks
			foreach (TexturedMesh item in Texturedmeshes)
			{
				bool placed = false;
				foreach (TextureBlock tb in TextureBlocks)
				{
					if (tb.Texturename == item.TextureName)
					{
						tb.meshes.Add(item.mesh);
						placed = true;
					}
				}

				if (placed == false)
				{
					TextureBlocks.Add(new TextureBlock(item));
				}
			}

			//sort textures by decreasing height
			TextureBlocks = SortTextures(TextureBlocks);

			// add textures to shelfs
			List<Shelf> shelfs = new List<Shelf>();
			shelfs.Add(new Shelf(maxwidth));

			bool blockplaced = false;
			foreach (TextureBlock texblock in TextureBlocks)
			{
				foreach (Shelf shelf in shelfs)
				{
					blockplaced = false;
					blockplaced = shelf.AddTexture(texblock);
				}
				if (blockplaced == false)
				{
					Shelf shelf = new Shelf(maxwidth);
					shelf.AddTexture(texblock);
					shelfs.Add(shelf);
				}
			}


			// combine shelfs into stacks of maxheight
			bool finished = false;
			List<Shelf> combinedShelfs;
			List<int> shelfids;
			while (finished == false)
			{
				combinedShelfs = new List<Shelf>();
				shelfids = new List<int>();
				int totalheight = 0;
				for (int i = 0; i < shelfs.Count; i++)
				{
					if (totalheight + shelfs[i].height < maxheight)
					{
						combinedShelfs.Add(shelfs[i]);
						shelfids.Add(i);
						totalheight += shelfs[i].height;
					}
				}
				shelfids.Reverse();
				foreach (int item in shelfids)
				{
					shelfs.RemoveAt(item);
				}

				output.Add(createTexturedMesh(combinedShelfs));

				if (shelfs.Count == 0) { finished = true; }
			}


			return output;
		}

		private static TexturedMesh createTexturedMesh(List<Shelf> shelfs)
		{
			int size = 2048;
			int totalheight = 0;
			foreach (Shelf shelf in shelfs)
			{
				totalheight += shelf.height;
			}
			Texture2D TotaalTexture = new Texture2D(size, totalheight, TextureFormat.RGBA32, false);
			int baseheight = 0;

			List<CombineInstance> submeshes = new List<CombineInstance>();

			//create the combined texture
			foreach (Shelf shelf in shelfs)
			{
				Color[] shelfpixels = shelf.ShelfTexture.GetPixels(0, 0, size, shelf.height, 0);
				TotaalTexture.SetPixels(0, baseheight, size, shelf.height, shelfpixels, 0);
				TotaalTexture.Apply(true);
				shelf.baseheight = baseheight;
				baseheight += shelf.height;
			}
			foreach (Shelf shelf in shelfs)
			{
				List<CombineInstance> shelfmeshes = shelf.ReturnMeshes(baseheight);
				foreach (CombineInstance item in shelfmeshes)
				{
					submeshes.Add(item);
				}

			}
			TexturedMesh output = new TexturedMesh();
			Mesh totalMesh = new Mesh();
			totalMesh.CombineMeshes(submeshes.ToArray(), true, false);
			foreach (var meshHolder in submeshes)
			{
				Destroy(meshHolder.mesh);
			}
			output.mesh = totalMesh;
			output.texture = TotaalTexture;

			return output;
		}
		public static TexturedMesh CombineMeshes(List<TexturedMesh> Texturedmeshes)
		{
			double totalArea = 0;
			double widest = 0;
			double highest = 0;
			foreach (TexturedMesh item in Texturedmeshes)
			{
				totalArea += item.texture.width * item.texture.height;
				if (item.texture.width > widest) { widest = item.texture.width; }
				if (item.texture.height > highest) { highest = item.texture.height; }
			}
			int size = GetMinimumSize(totalArea, widest, highest);
			if (size == (int)totalArea) { Debug.Log("afbeeldingen te groot"); return null; }


			List<TextureBlock> TextureBlocks = new List<TextureBlock>();
			// add meshes to textureblocks
			foreach (TexturedMesh item in Texturedmeshes)
			{
				bool placed = false;
				foreach (TextureBlock tb in TextureBlocks)
				{
					if (tb.Texturename == item.TextureName)
					{
						tb.meshes.Add(item.mesh);
						placed = true;
					}
				}

				if (placed == false)
				{
					TextureBlocks.Add(new TextureBlock(item));
				}
			}

			TextureBlocks = SortTextures(TextureBlocks);

			List<Shelf> shelfs = new List<Shelf>();
			shelfs.Add(new Shelf(size));

			bool blockplaced = false;
			foreach (TextureBlock texblock in TextureBlocks)
			{
				foreach (Shelf shelf in shelfs)
				{
					blockplaced = false;
					blockplaced = shelf.AddTexture(texblock);
				}
				if (blockplaced == false)
				{
					Shelf shelf = new Shelf(size);
					shelf.AddTexture(texblock);
					shelfs.Add(shelf);
				}
			}
			int totalheight = 0;
			foreach (Shelf shelf in shelfs)
			{
				totalheight += shelf.height;
			}
			Texture2D TotaalTexture = new Texture2D(size, totalheight);
			int baseheight = 0;

			List<CombineInstance> submeshes = new List<CombineInstance>();

			//create the combined texture
			foreach (Shelf shelf in shelfs)
			{
				Color[] shelfpixels = shelf.ShelfTexture.GetPixels(0, 0, size, shelf.height, 0);
				TotaalTexture.SetPixels(0, baseheight, size, shelf.height, shelfpixels, 0);
				TotaalTexture.Apply(true);
				shelf.baseheight = baseheight;
				baseheight += shelf.height;
			}
			foreach (Shelf shelf in shelfs)
			{
				List<CombineInstance> shelfmeshes = shelf.ReturnMeshes(baseheight);
				foreach (CombineInstance item in shelfmeshes)
				{
					submeshes.Add(item);
				}

			}
			TexturedMesh output = new TexturedMesh();
			Mesh totalMesh = new Mesh();
			totalMesh.CombineMeshes(submeshes.ToArray(), true, false);
			foreach (var meshHolder in submeshes)
			{
				Destroy(meshHolder.mesh);
			}
			output.mesh = totalMesh;
			output.texture = TotaalTexture;

			return output;
		}
		private static List<TextureBlock> SortTextures(List<TextureBlock> blocks)
		{
			TextureBlock tempblock;
			bool flipped = true;
			while (flipped == true)
			{
				flipped = false;
				for (int i = 0; i < blocks.Count - 1; i++)
				{

					if (blocks[i].height < blocks[i + 1].height)
					{
						tempblock = blocks[i];
						blocks[i] = blocks[i + 1];
						blocks[i + 1] = tempblock;
						flipped = true;
					}
				}
			}
			return blocks;
		}

		private static int GetMinimumSize(double Area, double widest, double highest)
		{
			double maximage;

			maximage = widest;

			double minSize = System.Math.Sqrt(Area);
			double size = 0;
			if (minSize < 128 && maximage < 128)
			{
				return 128;
			}
			if (minSize < 256 && maximage < 256)
			{
				return 256;
			}
			if (minSize < 512 && maximage < 512)
			{
				return 512;
			}
			if (minSize < 1024 && maximage < 1024)
			{
				return 1024;
			}
			if (minSize < 2048 && maximage < 2048)
			{
				return 2048;
			}
			if (maximage < 2048)
			{
				return 2048;
			}
			return (int)Area;
		}
	}

	public class Shelf
	{
		public int baseheight = 0;
		private int FloorPixelX;
		private int CeilingPixelX;
		public int pixelY;
		public int height;
		int width;
		public Texture2D ShelfTexture;
		public int[] AvailableWidth;
		List<TextureBlock> textureblocks = new List<TextureBlock>();

		public Shelf(int Width)
		{
			width = Width;
			FloorPixelX = 0;
			height = -1;
			CeilingPixelX = width;

		}

		public bool AddTexture(TextureBlock texture)
		{
			bool Added = false;
			if (CeilingPixelX < width)
			{
				Added = AddToCeiling(texture);
			}
			if (Added == false)
			{
				Added = AddToFloor(texture);
			}
			if (Added == false)
			{
				Added = AddToCeiling(texture);
			}
			return Added;

		}
		private bool AddToFloor(TextureBlock texture)
		{
			bool succes = false;
			if (height == -1)
			{
				height = texture.height;
				AvailableWidth = new int[height];
				for (int i = 0; i < AvailableWidth.Length; i++)
				{
					AvailableWidth[i] = width;
				}
				ShelfTexture = new Texture2D(width, height);
			}

			if (AvailableWidth[texture.height - 1] >= texture.width)
			{
				texture.NewStartX = FloorPixelX;
				texture.newStartY = 0;
				textureblocks.Add(texture);
				ShelfTexture.SetPixels(FloorPixelX, 0, texture.width, texture.height, texture.pixels, 0);
				ShelfTexture.Apply(true);
				FloorPixelX += texture.width;
				for (int i = 0; i < texture.height; i++)
				{
					AvailableWidth[i] -= texture.width;
				}
				succes = true;
			}

			return succes;
		}

		private bool AddToCeiling(TextureBlock texture)
		{
			bool succes = false;

			if (AvailableWidth[height - texture.height] >= texture.width)
			{
				texture.NewStartX = CeilingPixelX;
				texture.newStartY = height - texture.height;
				ShelfTexture.SetPixels(CeilingPixelX - texture.width, height - texture.height, texture.width, texture.height, texture.pixels, 0);
				ShelfTexture.Apply(true);
				CeilingPixelX -= texture.width;
				for (int i = height - 1; i > height - 1 - texture.height; i--)
				{
					AvailableWidth[i] -= texture.width;
				}
				succes = true;
				textureblocks.Add(texture);
			}

			return succes;
		}

		public List<CombineInstance> ReturnMeshes(int totalHeight)
		{
			List<CombineInstance> submeshes = new List<CombineInstance>();
			foreach (TextureBlock item in textureblocks)
			{
				List<Mesh> meshes = item.GetMeshes(baseheight, totalHeight, width);
				CombineInstance ci;
				foreach (Mesh submesh in meshes)
				{
					ci = new CombineInstance();
					ci.mesh = submesh;
					submeshes.Add(ci);
				}
			}
			return submeshes;
		}
	}
	public class TextureBlock
	{
		public string Texturename;
		public Texture2D texture;
		public int width;
		public int height;
		public int NewStartX;
		public int newStartY;
		public Color[] pixels;

		public List<Mesh> meshes;
		public TextureBlock(TexturedMesh texturedmesh)

		{
			Texturename = texturedmesh.TextureName;
			texture = texturedmesh.texture;
			width = texturedmesh.texture.width;
			height = texturedmesh.texture.height;
			pixels = texturedmesh.texture.GetPixels(0, 0, width, height);

			meshes = new List<Mesh>();
			meshes.Add(texturedmesh.mesh);
		}



		public List<Mesh> GetMeshes(int baseHeight, int totalHeight, int totalwidth)
		{
			if (meshes.Count == 0)
			{
				return null;
			}
			foreach (Mesh mesh in meshes)
			{
				if (mesh != null)
				{
					Vector2[] uvs = mesh.uv;
					for (int i = 0; i < uvs.Length; i++)
					{
						float pixelX = uvs[i].x * width;
						pixelX = (NewStartX + pixelX) / totalwidth;
						float pixelY = uvs[i].y * height;
						pixelY = (baseHeight + newStartY + pixelY) / totalHeight;

						uvs[i].x = pixelX;
						uvs[i].y = pixelY;
					}
					mesh.uv = uvs;

				}
			}

			return meshes;
		}
	}
}