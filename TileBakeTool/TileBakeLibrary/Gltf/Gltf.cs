/*
*  Copyright (C) X Gemeente
*              	 X Amsterdam
*				 X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://joinup.ec.europa.eu/software/page/eupl
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
namespace Netherlands3D.Gltf
{
	public class GltfRoot
	{
		public Asset asset { get; set; }
		public int scene { get; set; }
		public Scene[] scenes { get; set; }
		public Node[] nodes { get; set; }
		public Mesh[] meshes { get; set; }
		public Accessor[] accessors { get; set; }
		public Bufferview[] bufferViews { get; set; }
		public Buffer[] buffers { get; set; }
	}

	public class Asset
	{
		public string generator { get; set; }
		public string version { get; set; }
	}

	public class Scene
	{
		public string name { get; set; }
		public int[] nodes { get; set; }
	}

	public class Node
	{
		public int mesh { get; set; }
		public string name { get; set; }
		public float[] scale { get; set; }
	}

	public class Mesh
	{
		public string name { get; set; }
		public Primitive[] primitives { get; set; }
	}

	public class Primitive
	{
		public Attributes attributes { get; set; }
		public int indices { get; set; }
	}

	public class Attributes
	{
		public int POSITION { get; set; }
		public int NORMAL { get; set; }
	}

	public class Accessor
	{
		public int bufferView { get; set; }
		public int componentType { get; set; }
		public int count { get; set; }
		public float[] max { get; set; }
		public float[] min { get; set; }
		public string type { get; set; }
	}

	public class Bufferview
	{
		public int buffer { get; set; }
		public int byteLength { get; set; }
		public int byteOffset { get; set; }
	}

	public class Buffer
	{
		public long byteLength { get; set; }
		public string uri { get; set; }
	}
}