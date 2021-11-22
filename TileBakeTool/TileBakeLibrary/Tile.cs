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
		public Vector2Double position;
		public List<CityObject> cityObjects;

		public List<Vector3> vertices;
		public List<Vector3> normals;
		public List<Vector2> uvs;

		public List<int> triangleIndices;
	}
}
