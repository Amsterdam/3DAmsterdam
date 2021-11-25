using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TileBakeLibrary
{
	class BinaryMeshWriter
	{
        private static int writerVersion = 1;        

        public static void Save(Tile tile, bool writeMetaData = true, bool addGltfWrapper = true)
        {
            using (FileStream file = File.Create(tile.filePath))
            {
                using (BinaryWriter writer = new BinaryWriter(file))
                {
                    //Version int
                    writer.Write(writerVersion);

                    //Verts
                    var vertices = tile.vertices;
                    writer.Write(vertices.Count);
                    foreach (Vector3 vert in tile.vertices)
                    {
                        writer.Write(vert.X);
                        writer.Write(vert.Y);
                        writer.Write(vert.Z);
                    }

                    var normals = tile.normals;
                    //Normals
                    writer.Write(normals.Count);
                    foreach (Vector3 normal in normals)
                    {
                        writer.Write(normal.X);
                        writer.Write(normal.Y);
                        writer.Write(normal.Z);
                    }

                    //UV
                    var uvs = tile.uvs;
                    writer.Write(uvs.Count);
                    foreach (Vector2 uv in uvs)
                    {
                        writer.Write(uv.X);
                        writer.Write(uv.Y);
                    }

                    //Every triangle list per submesh
                    writer.Write(tile.submeshes.Count);
                    for (int i = 0; i < tile.submeshes.Count; i++)
                    {
                        List<int> submeshTriangleList = tile.submeshes[i].triangleIndices;
                        writer.Write(submeshTriangleList.Count);
                        writer.Write(tile.submeshes[i].baseVertex);
                        foreach (int index in submeshTriangleList)
                        {
                            writer.Write(index);
                        }
                    }
                }
            }

            //The metadata containing subobject id's and their index ranges
            if (writeMetaData)
            {
                using (FileStream file = File.Create(tile.filePath.Replace(".bin", "-data.bin")))
                {
                    using (BinaryWriter writer = new BinaryWriter(file))
                    {
                        //Version int
                        writer.Write(writerVersion);

                        //Subobject count
                        writer.Write(tile.SubObjects.Count);

                        //All subobject id's, and their indices
                        for (int i = 0; i < tile.SubObjects.Count; i++)
                        {
                            //ID string. string starts with a length:
                            //https://docs.microsoft.com/en-us/dotnet/api/system.io.binarywriter.write?view=net-5.0#System_IO_BinaryWriter_Write_System_String_
                            writer.Write(tile.SubObjects[i].id);

                            //Check how often this ID index appears in the vectormap (that is the vert indices count of the object)
                            int amountOfInts = tile.SubObjects[i].triangleIndices.Count;
                            writer.Write(amountOfInts);
                        }
                    }
                }
            }
        }
    }
}
