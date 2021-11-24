using System;
using System.Collections.Generic;
using System.Numerics;
using TileBakeLibrary.Coordinates;

namespace TileBakeLibrary
{
	class SubObject
	{
		public List<Vector3Double> vertices; 
		public List<Vector3> normals; 
		public List<Vector2> uvs; 
		public List<int> triangleIndices;

		public Vector2Double centroid;

		public string id = "";
		public int indicesLengthInTile;

		public void MergeSimilarVertices(float mergeVerticesBelowNormalAngle)
		{
			throw new NotImplementedException();
		}
	}
}
