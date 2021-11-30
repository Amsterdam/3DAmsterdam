using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TileBakeLibrary.Coordinates;

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
                    //Version
                    var version = reader.ReadInt32();

                    //Subobject count
                    var subObjects = reader.ReadInt32();

                    int objectOffset = 0;
                    //All subobject id's, and their indices
                    for (int i = 0; i < subObjects; i++)
                    {
                        var objectId = reader.ReadString();
                        var objectIndexRange = reader.ReadInt32();

                        var subMeshTriangleIndices = tile.submeshes[0].triangleIndices.GetRange(objectOffset,objectIndexRange);
                        var subObjectIndices = new int[objectIndexRange];

                        var subObjectVertices = new List<Vector3Double>();
                        var subObjectNormals = new List<Vector3>();
                        var subObjectUvs = new List<Vector2>();
                        for (int j = 0; j < subMeshTriangleIndices.Count; j++)
						{
                            var indexInSubMesh = subMeshTriangleIndices[j];
                            var indexInSubObject = subMeshTriangleIndices[j] - objectOffset;
                            subObjectIndices[j] = indexInSubObject;

                            if(indexInSubObject > subObjectVertices.Count)
                            {
                                var subMeshVertex = tile.vertices[indexInSubMesh];

                                //Add the subobject data (if we didnt already for this vert)
                                var vertex = new Vector3Double(subMeshVertex.X + tile.position.X + (tile.size.X / 2), subMeshVertex.Z + tile.position.Y + (tile.size.Y / 2), subMeshVertex.Y );
                                var normal = tile.normals[indexInSubMesh];
                                var uv = tile.uvs[indexInSubMesh];

                                subObjectVertices.Add(vertex);
                                subObjectNormals.Add(normal);
                                //subObjectUvs.Add(uv);
                            }
                        }

                        //We reverse per-object winding order when we write too, so here we reverse it back.
                        subObjectIndices.Reverse();

                        //Restore our subobject data
                        tile.AddSubObject(new SubObject()
                        {
                            id = objectId,
                            vertices = subObjectVertices,
                            normals = subObjectNormals,
                            uvs = subObjectUvs,
                            triangleIndices = subObjectIndices.ToList(),
                            parentSubmeshIndex = 0
                        },false);

                        //And clear the tile data again so we can rebuild it at bake time.

                        objectOffset += objectIndexRange;
                    }
                }
            }

            //Clear tile mesh data.
            tile.vertices.Clear();
            tile.normals.Clear();
            tile.uvs.Clear();
        }

		private static void Dictionary<T1, T2>()
		{
			throw new NotImplementedException();
		}
	}
}
