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
		private DMesh3 mesh;
		public float maxVerticesPerSquareMeter;
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


		private void createMesh()
        {
			mesh = new DMesh3(false, false, false, false);

			
			for (int i = 0; i < vertices.Count; i++)
			{
				mesh.AppendVertex(new Vector3d(vertices[i].X, vertices[i].Y, vertices[i].Z));

			}
			for (int i = 0; i < triangleIndices.Count; i += 3)
			{
				mesh.AppendTriangle(triangleIndices[i], triangleIndices[i + 1], triangleIndices[i + 2]);
			}
			MeshNormals.QuickCompute(mesh);
		}

		private void saveMesh()
        {
			vertices.Clear();
			WriteMesh outputMesh = new WriteMesh(mesh);
			int vertCount = outputMesh.Mesh.VertexCount;
			Vector3d vector;
			Vector3d normal;
			int[] mapV = new int[mesh.MaxVertexID];
			int nAccumCountV = 0;
			foreach (int vi in mesh.VertexIndices())
			{
				mapV[vi] = nAccumCountV++;
				Vector3d v = mesh.GetVertex(vi);
				vertices.Add(new Vector3Double(v.x, v.y, v.z));
				normal = mesh.GetVertexNormal(vi);
				normals.Add(new Vector3((float)normal.x, (float)normal.y, (float)normal.z));
			}

			triangleIndices.Clear();
			foreach (int ti in mesh.TriangleIndices())
			{
				Index3i t = mesh.GetTriangle(ti);
				triangleIndices.Add(mapV[t[0]]);
				triangleIndices.Add(mapV[t[1]]);
				triangleIndices.Add(mapV[t[2]]);
			}
			mesh = null;
		}

		public void SimplifyMesh()
        {
            if (mesh == null)
            {
				createMesh();
            }

			//MergeSimilarVertices(50);
			
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


			int edgecount = mesh.BoundaryEdgeIndices().Count(p=>p>-1);
			double area = MeshMeasurements.AreaT(mesh, mesh.TriangleIndices());
			int squareMetersPerVertex = 100;
            int maxSurfaceCount = (int)(area*maxVerticesPerSquareMeter)+edgecount;
            if (mesh.VertexCount > maxSurfaceCount)
            {
                r.ReduceToVertexCount(maxSurfaceCount);

            }

           mesh = r.Mesh;

			saveMesh();


		}

		public List<SubObject> clipSubobject()
        {
			List<SubObject> subObjects = new List<SubObject>();
			createMesh();
			var bounds = MeshMeasurements.Bounds(mesh, null);
			int rdXmin = (int)Math.Floor(bounds.Min.x/1000)*1000;
			int rdYmin = (int)Math.Floor(bounds.Min.y / 1000) * 1000;
			int rdXmax = (int)Math.Ceiling(bounds.Max.x / 1000) * 1000;
			int rdYmax = (int)Math.Ceiling(bounds.Max.y / 1000) * 1000;

            if (rdXmax-rdXmin==1000 && rdYmax-rdYmin==10000)
            {
				return subObjects;
            }

            for (int x = rdXmin; x < rdXmax; x += 1000)
            {
				DMesh3 columnMesh = CutColumnMesh(x);
                for (int y = rdYmin; y < rdYmax; y += 1000)
                {
                    SubObject newSubobject = clipMesh(columnMesh, x, y);
                    if (newSubobject != null)
                    {

                        subObjects.Add(newSubobject);
                    }
                }
            }


            return subObjects;
		}

		private DMesh3 CutColumnMesh(double X)
        {
			DMesh3 clippedMesh = new DMesh3(false, false, false, false);
			MeshPlaneCut mpc = new MeshPlaneCut(mesh, new Vector3d(X, 0, 0), new Vector3d(-1, 0, 0));
			mpc.Cut();
			clippedMesh = mpc.Mesh;
			//cut off the right side
			mpc = new MeshPlaneCut(clippedMesh, new Vector3d(X + 1000,0, 0), new Vector3d(1, 0, 0));
			mpc.Cut();
			clippedMesh = mpc.Mesh;
			return clippedMesh;
			//cut off the top
		}

		private SubObject clipMesh(DMesh3 columnMesh, double X, double Y)
        {
			SubObject subObject; 
			DMesh3 clippedMesh = new DMesh3(false, false, false, false);
			clippedMesh.Copy(columnMesh);


			//cut off the top
			MeshPlaneCut mpc = new MeshPlaneCut(clippedMesh, new Vector3d(X + 1000, Y + 1000, 0), new Vector3d(0, 1, 0));
            mpc.Cut();
            clippedMesh = mpc.Mesh;
            //cut off the bottom
            mpc = new MeshPlaneCut(clippedMesh, new Vector3d(X, Y, 0), new Vector3d(0, -1, 0));
            mpc.Cut();
            clippedMesh = mpc.Mesh;
            if (clippedMesh.VertexCount>0)
            {
				//create new subobject
				subObject = new SubObject();
				var center = MeshMeasurements.Centroid(clippedMesh);
				subObject.centroid = new Vector2Double(center.x, center.y);
				subObject.id = id;
				subObject.parentSubmeshIndex = parentSubmeshIndex;
				subObject.mesh = clippedMesh;
				subObject.saveMesh();

				return subObject;
            }

			return null;
        }
	}
}
