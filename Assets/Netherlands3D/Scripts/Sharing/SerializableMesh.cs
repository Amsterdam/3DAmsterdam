namespace Netherlands3D.Sharing{ 
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
		public string materialId;
		public int[] triangles; //vert ids, per 3
	}
}