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
			var radians = (Math.PI / 180) * mergeVerticesBelowNormalAngle;
			float cosAngleThreshold = (float)Math.Cos(radians);

			List<Vector3Double> cleanedVertices = new();
			List<Vector3> cleanedNormals = new();
			List<Vector2> cleanedUvs = new();

			//Traverse the triangles, and if we encounter verts+normals that are similar, dispose them
			for (int i = 0; i < triangleIndices.Count; i++)
			{
				var index = triangleIndices[i];
				triangleIndices[i] = GetOrAddVertexIndex(index ,cleanedVertices,cleanedNormals,cleanedUvs, cosAngleThreshold);
			}

			var merged = vertices.Count - cleanedVertices.Count;
			Console.WriteLine($"{id} has {merged} verts merged.");
			//Now use our new cleaned geometry
			vertices = cleanedVertices;
			normals = cleanedNormals;
			uvs = cleanedUvs;
		}

		private int GetOrAddVertexIndex(int index, List<Vector3Double> cleanedVertices, List<Vector3> cleanedNormals, List<Vector2> cleanedUvs, float angleThreshold)
		{
			Vector3Double inputVertex = vertices[index];
			Vector3 inputNormal = normals[index];
			//Vector2 inputUv = uvs[index]; //When we support uv's, a vertex with a unique UV should not be merged and be added as a unique one

		    //Add vertex with unique positions
			if(!cleanedVertices.Contains(inputVertex))
			{
				cleanedVertices.Add(inputVertex);
				cleanedNormals.Add(inputNormal);
				return cleanedVertices.Count - 1;
			}
			else 
			{
				//Compare the normal using a threshold
				var vertexIndex = cleanedVertices.IndexOf(inputVertex);
				var existingVertNormal = cleanedNormals[vertexIndex];
				if (Vector3.Dot(inputNormal, existingVertNormal) >= angleThreshold)
				{
					//Similar enough normal reuse existing vert
					return vertexIndex;
				}
				else{
					cleanedVertices.Add(inputVertex);
					cleanedNormals.Add(inputNormal);
					return cleanedVertices.Count - 1;
				}
			}
		}
	}
}
