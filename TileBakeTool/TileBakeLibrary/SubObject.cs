using System;
using System.Collections.Generic;
using System.Numerics;
using TileBakeLibrary.Coordinates;

namespace TileBakeLibrary
{
	class SubObject
	{
		public List<Vector3Double> vertices = new List<Vector3Double>(); 
		public List<Vector3> normals = new List<Vector3>();
		public List<Vector2> uvs = new List<Vector2>();
		public List<int> triangleIndices = new List<int>();

		public Vector2Double centroid = new Vector2Double();

		public int parentSubmeshIndex = 0;

		public string id = "";
		private double distanceMergeThreshold = 0.01;

		public void MergeSimilarVertices(float mergeVerticesBelowNormalAngle)
		{
			var radians = (Math.PI / 180) * mergeVerticesBelowNormalAngle;
			float cosAngleThreshold = (float)Math.Cos(radians);

			List<Vector3Double> cleanedVertices = new List<Vector3Double>();
			List<Vector3> cleanedNormals = new List<Vector3>();
			List<Vector2> cleanedUvs = new List<Vector2>();

			//Traverse the triangles, and if we encounter verts+normals that are similar, dispose them
			for (int i = 0; i < triangleIndices.Count; i++)
			{
				var vertexIndex = triangleIndices[i];
				triangleIndices[i] = GetOrAddVertexIndex(vertexIndex ,cleanedVertices,cleanedNormals,cleanedUvs, cosAngleThreshold);
			}

			//Now use our new cleaned geometry
			vertices = cleanedVertices;
			normals = cleanedNormals;
			uvs = cleanedUvs;
		}

		private int GetOrAddVertexIndex(int vertexIndex, List<Vector3Double> cleanedVertices, List<Vector3> cleanedNormals, List<Vector2> cleanedUvs, float angleThreshold)
		{
			Vector3Double inputVertex = vertices[vertexIndex];
			Vector3 inputNormal = normals[vertexIndex];
			//Vector2 inputUv = uvs[index]; //When we support uv's, a vertex with a unique UV should not be merged and be added as a unique one

			//Find vertex on a similar threshold position, and then normal
			for (int i = 0; i < cleanedVertices.Count; i++)
			{
				var cleanedVertex = cleanedVertices[i];
				var distance = Vector3Double.Distance(inputVertex, cleanedVertex);
				if(distance < distanceMergeThreshold)
				{
					//Compare the normal using a threshold
					var cleanedVertNormal = cleanedNormals[i];
					if (Vector3.Dot(inputNormal, cleanedVertNormal) >= angleThreshold)
					{
						//Similar enough normal reuse existing vert
						return i;
					}
				}
			}

			cleanedVertices.Add(inputVertex);
			cleanedNormals.Add(inputNormal);
			return cleanedVertices.Count - 1;
		}
	}
}
