using UnityEngine;

public partial class SubObjects
{
	[System.Serializable]
	public class SubOjectData{
		public string objectID;
		public int firstIndex;
		public int indicesLength;
		public int indicesCount;
		public int firstVertex;
		public int verticesLength;
		public Color color = Color.white;
		public bool hidden = false;
		public int subMeshID = 0;
	}
}
