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

		private List<SubObject> subObjects = new List<SubObject>();
		public List<SubObject> SubObjects { get => subObjects; }


		public void AddSubObject(SubObject subObject, int targetSubMeshIndex = 0)
		{
			//Make sure a submesh with the target index actualy exists
			CreateSubMesh(targetSubMeshIndex);

			subObjects.Add(subObject);
			AppendMeshData(subObject, targetSubMeshIndex);
		}

		private void CreateSubMesh(int targetSubMeshIndex)
		{
			Submesh subMesh;
			if (submeshes.Count == 0)
			{
				subMesh = new Submesh();
				submeshes.Add(subMesh);
			}
			else if (targetSubMeshIndex > submeshes.Count - 1)
			{
				var addSubMeshes = targetSubMeshIndex - (submeshes.Count - 1);
				for (int i = 0; i < addSubMeshes; i++)
				{
					subMesh = new Submesh();
					submeshes.Add(subMesh);
				}
			}
		}

		private void AppendMeshData(SubObject subObject, int targetSubMeshIndex = 0)
		{
			var indexOffset = vertices.Count;
			for (int i = 0; i < subObject.vertices.Count; i++)
			{
				var doubleVertex = subObject.vertices[i];
				//Here we convert to single precision, and switch the tile coordinate system
				var vertex = new Vector3((float)(doubleVertex.X - position.X), (float)doubleVertex.Z, (float)(doubleVertex.Y - position.Y));
				var normal = subObject.normals[i];

				vertices.Add(vertex);
				normals.Add(normal);
			}

			var targetSubMesh = submeshes[targetSubMeshIndex];
			for (int i = 0; i < subObject.triangleIndices.Count; i++)
			{
				targetSubMesh.triangleIndices.Add(indexOffset + subObject.triangleIndices[i]);
			}
		}
	}
}
