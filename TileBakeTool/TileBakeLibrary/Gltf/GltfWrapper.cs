using Bunny83.SimpleJSON;
using Netherlands3D.Gltf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TileBakeLibrary
{
	class GltfWrapper
	{
		private const int dataSlots = 2; //POSITION and NORMAL for now. TEXCOORD_0 would add 1 more if we add UV's
		private const int skipBelowVertices = 3;

		public static void Save(Tile tile)
		{
			GltfRoot gltfFile = new GltfRoot();

			//Lets create a gltf with some basic info and one scene
			gltfFile.asset = new Asset()
			{
				generator = "Netherlands3D Tile Bake Tool",
				version = "2.0"
			};
			gltfFile.scene = 0;
			gltfFile.scenes = new Scene[1]{
				new Scene()
				{
					name = "Scene",
				}
			};
			//And add one external binary file as buffer file. ( our own binary tile file )
			FileInfo fileInfo = new FileInfo(tile.filePath);
			gltfFile.buffers = new Netherlands3D.Gltf.Buffer[1]
			{
				new Netherlands3D.Gltf.Buffer(){
					byteLength = fileInfo.Length,
					uri = Path.GetFileName(tile.filePath) //Relative path (just the filename)
                }
			};

			//Now add our tile submeshes to the scene as new nodes+meshes
			AddNodesAndSubmeshes(tile, gltfFile);

			//Create the accessors that determine the interpretation of binary data for our objects
			AddAccessorsForBinaryMeshData(tile, gltfFile);

			//Add the ranges of binary data blocks ( with proper offsets/margins )
			CreateBufferViews(tile, gltfFile);

			//Write pretty formatted JSON, and ignore null fields
			string jsonString = JsonSerializer.Serialize(gltfFile, new JsonSerializerOptions()
			{
				WriteIndented = true,
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
			});

			File.WriteAllText(tile.filePath + ".gltf", jsonString);
		}

		private static void CreateBufferViews(Tile tile, GltfRoot gltfFile)
		{
			var byteOffset = 8; //Skip version nr and vertex count bytes
			var verticesByteLength = tile.vertices.Count * 3 * 4; //3 (xyz) * 4 float bytes

			var normalsByteOffset = byteOffset + 4 + verticesByteLength;
			var normalsLength = tile.normals.Count * 3 * 4; //3 (xyz) * 4 float bytes

			var uvsByteOffset = normalsByteOffset + 4 + normalsLength;
			var uvsLength = tile.uvs.Count * 2 * 4; //3 (xy) * 4 float bytes

			var indicesByteOffset = uvsByteOffset + 4 + uvsLength;
			var indicesLength = 0;

			//Complete view for vertices and normals. Unique view per submesh for its indices.
			List<Bufferview> bufferViews = new List<Bufferview>();

			var verticesView = new Bufferview()
			{
				buffer = 0,
				byteLength = verticesByteLength,
				byteOffset = byteOffset
			};
			var normalsView = new Bufferview()
			{
				buffer = 0,
				byteLength = normalsLength,
				byteOffset = normalsByteOffset
			};
			bufferViews.Add(verticesView);
			bufferViews.Add(normalsView);

			//Now a unique indices view per submesh
			for (int i = 0; i < tile.submeshes.Count; i++)
			{
				var subMesh = tile.submeshes[i];

				indicesByteOffset = indicesByteOffset + 8 + indicesLength; //skip triangles length and base vertex
				indicesLength = subMesh.triangleIndices.Count * 4; //4 bytes per Uint32

				if (subMesh.triangleIndices.Count > skipBelowVertices)
				{
					bufferViews.Add(new Bufferview()
					{
						buffer = 0,
						byteLength = indicesLength,
						byteOffset = indicesByteOffset
					});
				}
			}

			gltfFile.bufferViews = bufferViews.ToArray();
		}

		private static void AddAccessorsForBinaryMeshData(Tile tile, GltfRoot gltfFile)
		{
			//We always have two accessors for our big list of vertices and normals
			List<Accessor> accessors = new List<Accessor>();
			var vertices = new Accessor()
			{
				bufferView = 0,
				componentType = 5126,
				count = tile.vertices.Count,
				max = new float[3] { (float)tile.size.X, (float)tile.size.Y, (float)tile.size.Y },
				min = new float[3] { (float)-tile.size.X, (float)-tile.size.Y, (float)-tile.size.Y },
				type = "VEC3"
			};
			var normals = new Accessor()
			{
				bufferView = 1,
				componentType = 5126,
				count = tile.normals.Count,
				type = "VEC3"
			};
			accessors.Add(vertices);
			accessors.Add(normals);

			//And add a unique accessor per submesh (if they have triangles)
			var subMeshWithDataIndex = 0;
			for (int i = 0; i < tile.submeshes.Count; i++)
			{
				if (tile.submeshes[i].triangleIndices.Count > skipBelowVertices)
				{
					var submesh = tile.submeshes[i];
					var indices = new Accessor()
					{
						bufferView = 2 + subMeshWithDataIndex,
						componentType = 5125,
						count = submesh.triangleIndices.Count,
						type = "SCALAR"
					};
					accessors.Add(indices);
					subMeshWithDataIndex++;
				}
			}
			gltfFile.accessors = accessors.ToArray();
		}

		private static void AddNodesAndSubmeshes(Tile tile, GltfRoot gltfFile)
		{
			var subMeshes = tile.submeshes.Count;

			List<Node> nodes = new List<Node>();
			List<Mesh> meshes = new List<Mesh>();

			int nodeMeshIndex = 0;
			for (int i = 0; i < subMeshes; i++)
			{
				var submesh = tile.submeshes[i];
				if (submesh.triangleIndices.Count > skipBelowVertices)
				{
					var node = new Node()
					{
						mesh = nodes.Count,
						name = $"Node-{nodeMeshIndex}_{tile.position.X}_{tile.position.Y}",
						scale = new float[3] { 1.0f, 1.0f, -1.0f } //Gltf uses Z in the opposite direction
					};
					var subMesh = new Mesh()
					{
						name = $"Submesh-{nodeMeshIndex}_{tile.position.X}_{tile.position.Y}",
						primitives = new Primitive[1]
						{
								new Primitive()
								{
									attributes = new Attributes(){
										POSITION=0,
										NORMAL=1
									},
									indices=nodeMeshIndex+dataSlots
								}
						}
					};

					nodes.Add(node);
					meshes.Add(subMesh);

					nodeMeshIndex++;
				}
			}

			gltfFile.nodes = nodes.ToArray();
			gltfFile.meshes = meshes.ToArray();

			//Add incremental list of index nodes
			gltfFile.scenes[0].nodes = new int[gltfFile.nodes.Length];
			for (int i = 0; i < gltfFile.scenes[0].nodes.Length; i++)
				gltfFile.scenes[0].nodes[i] = i;
		}
	}
}