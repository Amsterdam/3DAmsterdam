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

        public static void SaveAsBinaryFile(Tile tile, string filePath, bool addGltfWrapper = true)
        {
            using (FileStream file = File.Create(filePath))
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
        }
    }
}
