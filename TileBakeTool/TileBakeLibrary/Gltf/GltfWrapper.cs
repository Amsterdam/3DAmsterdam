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
        public static void Save(Tile tile)
        {
            GltfRoot gltfFile = new GltfRoot();
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
                    nodes = new int[1] {
                        0
					}
                }
            };
            gltfFile.nodes = new Node[1]{
                new Node(){
                    mesh=0,
                    name="Tile-"+tile.position.X+"_"+tile.position.Y,
                    scale = new float[3]{ 1.0f, 1.0f, -1.0f } //Gltf uses Z in the opposite direction
                }
            };
            gltfFile.meshes = new Mesh[1]{
                new Mesh(){
                    name="Tile-"+tile.position.X+"_"+tile.position.Y,
                    primitives= new Primitive[1]{
                        new Primitive()
                        {
                            attributes = new Attributes(){
                                POSITION=0,
                                NORMAL=1
							},
                            indices=2
						}
					}
                }
            };
            gltfFile.accessors = new Accessor[3]
            {
                new Accessor()
                {
                    bufferView = 0,
                    componentType = 5126,
                    count = tile.vertices.Count,
                    max= new float[3]{ (float)tile.size.X,(float)tile.size.Y, (float)tile.size.Y },
                    min=new float[3]{ (float)-tile.size.X,(float)-tile.size.Y, (float)-tile.size.Y },
                    type="VEC3"
                },
                new Accessor()
                {
                    bufferView = 1,
                    componentType = 5126,
                    count = tile.normals.Count,
                    type="VEC3"
                },
                new Accessor()
                {
                    bufferView = 2,
                    componentType = 5125,
                    count = tile.submeshes[0].triangleIndices.Count,
                    type="SCALAR"
                }
            };


            var byteOffset = 8; //Skip version nr and vertex count bytes
            var verticesByteLength = tile.vertices.Count * 3 * 4; //3 (xyz) * 4 float bytes

            var normalsByteOffset = byteOffset + 4 + verticesByteLength;
            var normalsLength = tile.normals.Count * 3 * 4; //3 (xyz) * 4 float bytes

            var uvsByteOffset = normalsByteOffset + 4 + normalsLength;
            var uvsLength = tile.uvs.Count * 2 * 4; //3 (xy) * 4 float bytes

            var indicesByteOffset = uvsByteOffset + 12 + uvsLength; //Skip submesh, trianglecount and basevertex
            var indicesLength = tile.submeshes[0].triangleIndices.Count * 4; //4 bytes per Uint32


            gltfFile.bufferViews = new Bufferview[3]
            {
                new Bufferview(){
                    buffer=0,
                    byteLength = verticesByteLength,
                    byteOffset=byteOffset
                },
                new Bufferview(){
                    buffer=0,
                    byteLength=normalsLength,
                    byteOffset=normalsByteOffset
                },
                new Bufferview(){
                    buffer=0,
                    byteLength=indicesLength,
                    byteOffset=indicesByteOffset
                }
            };

            FileInfo fileInfo = new FileInfo(tile.filePath);
            gltfFile.buffers = new Netherlands3D.Gltf.Buffer[1]
            {
                new Netherlands3D.Gltf.Buffer(){
                    byteLength = fileInfo.Length,
                    uri = Path.GetFileName(tile.filePath) //Relative path (just the filename)
                }
            };

            //Write pretty formatted JSON, and ignore null fields
            string jsonString = JsonSerializer.Serialize(gltfFile, new JsonSerializerOptions() 
            { 
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            File.WriteAllText(tile.filePath+".gltf", jsonString);
        }
    }
}