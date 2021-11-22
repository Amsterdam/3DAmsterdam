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
        public static void SaveAsBinaryFile(int version, Tile tile, string filePath)
        {
            using (FileStream file = File.Create(filePath))
            {
                using (BinaryWriter writer = new BinaryWriter(file))
                {
                    //Version int
                    writer.Write(version);

                    //Verts
                    var vertices = tile.vertices;
                    writer.Write(vertices.Length);
                    foreach (Vector3 vert in tile.vertices)
                    {
                        writer.Write(vert.x);
                        writer.Write(vert.y);
                        writer.Write(vert.z);
                    }

                    var normals = tile.normals;
                    //Normals
                    writer.Write(normals.Length);
                    foreach (Vector3 normal in normals)
                    {
                        writer.Write(normal.x);
                        writer.Write(normal.y);
                        writer.Write(normal.z);
                    }

                    //UV
                    var uvs = tile.uv;
                    writer.Write(uvs.Length);
                    foreach (Vector2 uv in uvs)
                    {
                        writer.Write(uv.x);
                        writer.Write(uv.y);
                    }

                    //Every triangle list per submesh
                    writer.Write(tile.subMeshCount);
                    for (int i = 0; i < tile.subMeshCount; i++)
                    {
                        int[] submeshTriangleList = tile.GetTriangles(i);
                        writer.Write(submeshTriangleList.Length);
                        writer.Write(tile.GetSubMesh(i).baseVertex);
                        //var offset = sourceMesh.GetSubMesh(i).baseVertex;
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
