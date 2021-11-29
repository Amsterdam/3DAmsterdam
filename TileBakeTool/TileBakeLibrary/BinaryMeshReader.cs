using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TileBakeLibrary
{
	class BinaryMeshReader
	{
        public static void ReadBinaryFile(Tile tile)
        {
            var binaryMeshFile = File.ReadAllBytes(tile.filePath);
            var binaryMetaFile = File.ReadAllBytes(tile.filePath.Replace(".bin", "-data.bin"));

            //Read the mesh data into our tile
            using (var stream = new MemoryStream(binaryMeshFile))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    var version = reader.ReadInt32();
                    var vertLength = reader.ReadInt32();
                    Vector3[] vertices = new Vector3[vertLength];
                    for (int i = 0; i < vertLength; i++)
                    {
                        Vector3 vertex = new Vector3(
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle()
                         );
                        vertices[i] = vertex;
                    }
                    tile.vertices = vertices.ToList();

                    var normalsLength = reader.ReadInt32();
                    Vector3[] normals = new Vector3[normalsLength];
                    for (int i = 0; i < normalsLength; i++)
                    {
                        Vector3 normal = new Vector3(
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle()
                         );
                        normals[i] = normal;
                    }
                    tile.normals = normals.ToList();

                    var uvLength = reader.ReadInt32();
                    Vector2[] uvs = new Vector2[uvLength];
                    for (int i = 0; i < uvLength; i++)
                    {
                        Vector2 uv = new Vector2(
                            reader.ReadSingle(),
                            reader.ReadSingle()
                         );
                        uvs[i] = uv;
                    }
                    tile.uvs = uvs.ToList();

                    //Submeshes
                    var submeshes = reader.ReadInt32();
                    tile.submeshes = new List<Tile.Submesh>(submeshes);

                    //Debug.Log("Submeshes: " + submeshes);
                    for (int i = 0; i < submeshes; i++)
                    {
                        var trianglesLength = reader.ReadInt32();
                        var baseVertex = reader.ReadInt32();
                        var indices = new List<int>();
                        for (int j = 0; j < trianglesLength; j++)
                        {
                            var index = reader.ReadInt32();
                            indices.Add(index);
                        }
                        tile.submeshes.Add(new Tile.Submesh()
                        {
                            baseVertex = baseVertex,
                            triangleIndices = indices
                        });
                    }
                }
            }

            using (var stream = new MemoryStream(binaryMetaFile))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    var version = reader.ReadInt32();

                    //Subobject count
                    var subObjects = reader.ReadInt32();

                    int objectOffset = 0;
                    //All subobject id's, and their indices
                    for (int i = 0; i < subObjects; i++)
                    {
                        //ID string. string starts with a length:
                        //https://docs.microsoft.com/en-us/dotnet/api/system.io.binarywriter.write?view=net-5.0#System_IO_BinaryWriter_Write_System_String_
                        var id = reader.ReadString();
                        var objectIndexRange = reader.ReadInt32();

                        var indices = tile.vertices.GetRange(objectOffset, objectIndexRange);

                        var objectVertices = tile.vertices.GetRange(objectOffset, objectIndexRange);
                        var normals =
                        var uvs =

                        tile.AddSubObject(new SubObject()
                        {
                            triangleIndices = 
                        },0);

                        objectOffset += objectIndexRange;
                    }
                }
            }
        }
    }
}
