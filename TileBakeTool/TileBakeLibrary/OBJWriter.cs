using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TileBakeLibrary
{
	class OBJWriter
	{
        public static void Save(Tile tile, bool addGltfWrapper = true)
        {
            // needs adapting to binaryMeshDAta

      //      using (FileStream file = File.Create(tile.filePath + ".obj"))
      //      {
      //          using (StreamWriter writer = new StreamWriter(file))
      //          {
      //              writer.WriteLine($"#NL3D Simple OBJ");
      //              //Verts
      //              var vertices = tile.vertices;
      //              foreach (Vector3 vert in tile.vertices)
      //              {
      //                  writer.WriteLine($"v {vert.X} {vert.Y} {vert.Z}");
      //              }

      //              var normals = tile.normals;
      //              //Normals
      //              foreach (Vector3 normal in normals)
      //              {
      //                  writer.WriteLine($"vn {normal.X} {normal.Y} {normal.Z}");
      //              }

      //              //UV
      //              var uvs = tile.uvs;
      //              foreach (Vector2 uv in uvs)
      //              {
      //                  writer.WriteLine($"vt {uv.X} {uv.Y}");
      //              }

      //              //Every triangle list per submesh
      //              writer.WriteLine("o SubMesh");
      //              for (int i = 0; i < tile.submeshes.Count; i++)
      //              {
      //                  List<int> submeshTriangleList = tile.submeshes[i].triangleIndices;
						//for (int j = 0; j < submeshTriangleList.Count; j+=3)
						//{
      //                      var index1 = submeshTriangleList[j] + 1;
      //                      var index2 = submeshTriangleList[j + 1] + 1;
      //                      var index3 = submeshTriangleList[j + 2] + 1;
      //                      writer.WriteLine($"f {index1}/{((uvs.Count>0) ? index1 : "")}/{index1} {index2}/{((uvs.Count > 0) ? index2 : "")}/{index2} {index3}/{((uvs.Count > 0) ? index3 : "")}/{index3}");
      //                  }
      //              }
      //          }
      //      }
        }
    }
}
