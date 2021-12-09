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
using Bunny83.SimpleJSON;
using Netherlands3D.Gltf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TileBakeLibrary.BinaryMesh;

namespace TileBakeLibrary
{
	class GltfWrapper
	{
		private const int dataSlots = 2; //POSITION and NORMAL for now. TEXCOORD_0 would add 1 more if we add UV's
		private const int skipBelowVertices = 3;

		public static void Save(MeshData mesh, string filename, string binaryFilename)
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
			FileInfo fileInfo = new FileInfo(binaryFilename);
			gltfFile.buffers = new Netherlands3D.Gltf.Buffer[1]
			{
				new Netherlands3D.Gltf.Buffer(){
					byteLength = fileInfo.Length,
					uri = Path.GetFileName(binaryFilename) //Relative path (just the filename)
                }
			};

			//Now add our tile submeshes to the scene as new nodes+meshes
			AddNodesAndSubmeshes(mesh, gltfFile);

			//Create the accessors that determine the interpretation of binary data for our objects
			AddAccessorsForBinaryMeshData(mesh, gltfFile);

			//Add the ranges of binary data blocks ( with proper offsets/margins )
			CreateBufferViews(mesh, gltfFile);

			//Write pretty formatted JSON, and ignore null fields
			string jsonString = JsonSerializer.Serialize(gltfFile, new JsonSerializerOptions()
			{
				WriteIndented = true,
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
			});
            if (checkoutput(gltfFile))
            {
				File.WriteAllText(filename, jsonString);
			}
			else
            {
				Console.WriteLine($"err writing file {filename}");
            }
			
		}

		private static bool checkoutput(GltfRoot gltfFile)
        {
			var foundbufferlength = gltfFile.buffers[0].byteLength;
			var bufferviewcount = gltfFile.bufferViews.Count();
			var lastBufferview = gltfFile.bufferViews[bufferviewcount - 1];
			var totalbytespointedat = lastBufferview.byteLength + lastBufferview.byteOffset;
            if (totalbytespointedat>foundbufferlength)
            {
				return false;
            }
			return true;
        }

		private static void CreateBufferViews(MeshData mesh, GltfRoot gltfFile)
		{
			var byteOffset = 24; //Skip the header (6 * 4 bytes per int32) 
			var verticesByteLength = mesh.vertexCount * 3 * 4; //3 (xyz) * 4 float bytes

			var normalsByteOffset = byteOffset +  verticesByteLength;
			var normalsLength = mesh.normalsCount * 3 * 4; //3 (xyz) * 4 float bytes

			var uvsByteOffset = normalsByteOffset +  normalsLength;
			var uvsLength = 0 * 2 * 4; //3 (xy) * 4 float bytes

			var indicesByteOffset = uvsByteOffset +  uvsLength;
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
			for (int i = 0; i < mesh.submeshes.Count; i++)
			{
				var subMesh = mesh.submeshes[i];
				
				indicesByteOffset = indicesByteOffset + indicesLength;
				indicesLength = subMesh.indexcount * 4; //4 bytes per Uint32

				if (subMesh.indexcount> skipBelowVertices)
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

		private static void AddAccessorsForBinaryMeshData(MeshData mesh, GltfRoot gltfFile)
		{
			//We always have two accessors for our big list of vertices and normals
			List<Accessor> accessors = new List<Accessor>();
			var vertices = new Accessor()
			{
				bufferView = 0,
				componentType = 5126,
				count = mesh.vertexCount,
				max = new float[3] { 500,500,500},
				min = new float[3] {-500,-500,-500},
				type = "VEC3"
			};
			var normals = new Accessor()
			{
				bufferView = 1,
				componentType = 5126,
				count = mesh.normalsCount,
				type = "VEC3"
			};
			accessors.Add(vertices);
			accessors.Add(normals);

			//And add a unique accessor per submesh (if they have triangles)
			var subMeshWithDataIndex = 0;
			for (int i = 0; i < mesh.submeshes.Count; i++)
			{
				if (mesh.submeshes[i].vertexcount > skipBelowVertices)
				{
					var submesh = mesh.submeshes[i];
					var indices = new Accessor()
					{
						bufferView = 2 + subMeshWithDataIndex,
						componentType = 5125,
						count = submesh.indexcount,
						type = "SCALAR"
					};
					accessors.Add(indices);
					subMeshWithDataIndex++;
				}
			}
			gltfFile.accessors = accessors.ToArray();
		}

		private static void AddNodesAndSubmeshes(MeshData mesh, GltfRoot gltfFile)
		{
			var subMeshes = mesh.submeshes.Count;

			List<Node> nodes = new List<Node>();
			List<Mesh> meshes = new List<Mesh>();

			int nodeMeshIndex = 0;
			for (int i = 0; i < subMeshes; i++)
			{
				var submesh = mesh.submeshes[i];
				if (submesh.indexcount > skipBelowVertices)
				{
					var node = new Node()
					{
						mesh = nodes.Count,
						name = $"Node-{nodeMeshIndex}",
						scale = new float[3] { 1.0f, 1.0f, -1.0f } //Gltf uses Z in the opposite direction
					};
					var subMesh = new Mesh()
					{
						name = $"Submesh-{nodeMeshIndex}",
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