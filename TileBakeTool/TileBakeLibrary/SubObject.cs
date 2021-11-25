using System;
using System.Collections.Generic;
using System.Numerics;
using TileBakeLibrary.Coordinates;

namespace TileBakeLibrary
{
	class SubObject
	{
		public List<Vector3Double> vertices = new(); 
		public List<Vector3> normals = new();
		public List<Vector2> uvs = new();
		public List<int> triangleIndices = new();

		public Vector2Double centroid = new();

		public string id = "";

		public void MergeSimilarVertices(float mergeVerticesBelowNormalAngle)
		{
			throw new NotImplementedException();
		}
	}
}
