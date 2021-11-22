using System.Collections.Generic;
using System.Numerics;
using TileBakeLibrary.Coordinates;

namespace TileBakeLibrary
{
	class SubObject
	{
		public List<Vector3Double> verticesRD; 
		public List<Vector3> normals; 
		public List<Vector3> uvs; 
		public List<int> triangleIndices;

		public Vector3Double centroid;

		public string id = "";
		public int indicesLengthInTile;
	}
}
