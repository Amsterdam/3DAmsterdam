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

			var indicesByteOffset = 0;
			var indicesLength = 0;

			//Complete view for vertices and normals. Unique view per submesh for its indices.
			gltfFile.bufferViews = new Bufferview[2 + tile.submeshes.Count];
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
			gltfFile.bufferViews[0] = verticesView;
			gltfFile.bufferViews[1] = normalsView;

			//Now a unique indices view per submesh
			for (int i = 0; i < tile.submeshes.Count; i++)
			{
				var subMesh = tile.submeshes[i];

				indicesByteOffset = uvsByteOffset + 12 + uvsLength; //Skip submesh, trianglecount and basevertex
				indicesLength = subMesh.triangleIndices.Count * 4; //4 bytes per Uint32

				gltfFile.bufferViews[i + 2] = new Bufferview()
				{
					buffer = 0,
					byteLength = indicesLength,
					byteOffset = indicesByteOffset
				};
			}
		}

		private static void AddAccessorsForBinaryMeshData(Tile tile, GltfRoot gltfFile)
		{
			//We always have two accessors for our big list of vertices and normals
			gltfFile.accessors = new Accessor[tile.submeshes.Count + 2];
			var vertices = new Accessor()
			{
				bufferView = 0,
				componentType = 5126,
				count = tile.vertices.Count,
				max = new float[3] { (float)tile.size.X, (float)tile.size.Y, (float)tile.size.Y },
				min = new float[3] { (float)-tile.size.X, (float)-tile.size.Y, (float)-tile.size.Y },
				type = "VEC3"
			};
			var normals = gltfFile.accessors[1] = new Accessor()
			{
				bufferView = 1,
				componentType = 5126,
				count = tile.normals.Count,
				type = "VEC3"
			};
			gltfFile.accessors[0] = vertices;
			gltfFile.accessors[1] = normals;

			//And add a unique accessor per submesh for their indices 
			for (int i = 0; i < tile.submeshes.Count; i++)
			{
				var indices = new Accessor()
				{
					bufferView = 2,
					componentType = 5125,
					count = tile.submeshes[i].triangleIndices.Count,
					type = "SCALAR"
				};
				gltfFile.accessors[i+2] = indices;
			}
		}

		private static void AddNodesAndSubmeshes(Tile tile, GltfRoot gltfFile)
		{
			var subMeshes = tile.submeshes.Count;
			gltfFile.nodes = new Node[subMeshes];
			gltfFile.meshes = new Mesh[subMeshes];
			gltfFile.scenes[0].nodes = new int[tile.submeshes.Count];

			for (int i = 0; i < subMeshes; i++)
			{
				var node = new Node() {
					mesh = i,
					name = $"Node-{i}_{tile.position.X}_{tile.position.Y}",
					scale = new float[3] { 1.0f, 1.0f, -1.0f } //Gltf uses Z in the opposite direction
				};
				var subMesh = new Mesh()
				{
					name = $"Submesh-{i}_{tile.position.X}_{tile.position.Y}",
					primitives = new Primitive[1]
					{
							new Primitive()
							{
								attributes = new Attributes(){
									POSITION=0,
									NORMAL=1
								},
								indices=i+dataSlots
							}
					}
				};

				gltfFile.nodes[i] = node;
				gltfFile.meshes[i] = subMesh;

				gltfFile.scenes[0].nodes[i] = i; //Just add all these nodes directly to our single scene
			}
			
		}
	}
}