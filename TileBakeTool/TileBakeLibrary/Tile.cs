using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TileBakeLibrary.Coordinates;

namespace TileBakeLibrary
{
	class Tile
	{
		public Vector2Double position = new Vector2Double(); //Bottom left (RD coordinates)
		public Vector2 size = new Vector2(); //Width and height (RD coordinates)

		public List<SubObject> subObjects = new List<SubObject>();

		public List<Vector3> vertices = new List<Vector3>();
		public List<Vector3> normals = new List<Vector3>();
		public List<Vector2> uvs = new List<Vector2>();

		public List<Submesh> submeshes = new List<Submesh>();

		public string filePath = "";

		public class Submesh
		{
			public List<int> triangleIndices;
			public int baseVertex;
		}
	}
}
