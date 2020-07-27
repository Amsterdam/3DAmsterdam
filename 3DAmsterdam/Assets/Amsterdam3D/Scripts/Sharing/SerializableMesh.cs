namespace Amsterdam3D.Sharing{ 
	[System.Serializable]
	public struct SerializableMesh{
		public string sceneId;
		public string meshToken;

		public string version;
		public int meshBitType;
		public SerializableSubMesh[] subMeshes;
		public float[] verts;
		public float[] uvs;
		public float[] normals;
	}
	[System.Serializable]
	public struct SerializableSubMesh
	{
		public string materialId; //we probably dont need this because material slot order is tied to submesh index order
		public int[] triangles; //vert ids, per 3
	}
}