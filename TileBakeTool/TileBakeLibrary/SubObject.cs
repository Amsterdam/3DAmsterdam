using System;
using System.Collections.Generic;
using System.Numerics;
using TileBakeLibrary.Coordinates;
using g3;
using gs;
using System.Linq;

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
			bool hasnormals = true;
            if (cleanedNormals.Count==0)
            {
				hasnormals = false;
            }
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
                    if (!hasnormals)
                    {
						return i;
                    }
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

		public void SimplifyMesh()
        {
			//MergeSimilarVertices(50);
			DMesh3 mesh = new DMesh3(false,false,false,false);
			
			List<Vector3d> DMeshVertices = new List<Vector3d>();
            for (int i = 0; i < vertices.Count; i++)
            {
				mesh.AppendVertex(new Vector3d(vertices[i].X, vertices[i].Y, vertices[i].Z));

			}
            for (int i = 0; i < triangleIndices.Count; i+=3)
            {
				mesh.AppendTriangle(triangleIndices[i], triangleIndices[i + 1], triangleIndices[i + 2]);

			}
			MeshNormals.QuickCompute(mesh);
            MergeCoincidentEdges merg = new MergeCoincidentEdges(mesh);
            merg.Apply();
            if (!mesh.CheckValidity(true, FailMode.ReturnOnly))
            {
                return;
            }

            // setup up the reducer
            Reducer r = new Reducer(mesh);
            // set reducer to preserve bounds

            r.SetExternalConstraints(new MeshConstraints());
            MeshConstraintUtil.FixAllBoundaryEdges(r.Constraints, mesh);

            // figure out desired triangleCount
            int maxSurfaceCount = (int)(0.05 * mesh.VertexCount);
            if (mesh.VertexCount > maxSurfaceCount)
            {
                r.ReduceToVertexCount(maxSurfaceCount);

            }

           mesh = r.Mesh;

            vertices.Clear();
			WriteMesh outputMesh = new WriteMesh(mesh);
			int vertCount = outputMesh.Mesh.VertexCount;
			Vector3d vector;
			int[] mapV = new int[mesh.MaxVertexID];
			int nAccumCountV = 0;
			foreach (int vi in mesh.VertexIndices())
            {
				mapV[vi] = nAccumCountV++;
				Vector3d v = mesh.GetVertex(vi);
				vertices.Add(new Vector3Double(v.x, v.y, v.z));
			}

			triangleIndices.Clear();
			foreach (int ti in mesh.TriangleIndices())
			{
				Index3i t = mesh.GetTriangle(ti);
				triangleIndices.Add( mapV[t[0]]);
				triangleIndices.Add(mapV[t[1]]);
				triangleIndices.Add(mapV[t[2]]);
			}
				//for (int i = 0; i < vertCount; i++)
    //        {
				//vector = outputMesh.Mesh.GetVertex(i);
				//vertices.Add(new Vector3Double(vector.x, vector.y, vector.z));
    //        }

			
			//int triangleCount = outputMesh.Mesh.TriangleCount;
			//Index3i index;
   //         for (int i = 0; i < triangleCount; i++)
   //         {
			//	index = outputMesh.Mesh.GetTriangle(i);
			//	triangleIndices.Add(index.a);
			//	triangleIndices.Add(index.b);
			//	triangleIndices.Add(index.c);
			//}
			
			
			
           
			//List<WriteMesh> meshes = new List<WriteMesh>();
			//meshes.Add(outputMesh);
			
			//var writeResult = StandardMeshWriter.WriteFile("E:/brondata/terreintest/output/"+id+".obj",
			//	meshes, new WriteOptions());

		}
	}
}
