using System.Collections.Generic;
using System.Numerics;

namespace TileBakeLibrary
{
	class CityObject
	{
		public string id = "";
		public List<Vector3> vertices; 
		public List<Vector3> normals; 
		public List<Vector3> uvs; 
		public List<int> triangleIndices;

		public Vector2 centroid;
	}
}
