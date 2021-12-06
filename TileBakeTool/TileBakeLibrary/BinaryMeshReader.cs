using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TileBakeLibrary.Coordinates;
using TileBakeLibrary.BinaryMesh;

namespace TileBakeLibrary
{
	class BinaryMeshReader
	{
        public static MeshData ReadBinaryMesh(string filename)
        {
            MeshData mesh = new MeshData();

            var binaryMeshFile = File.ReadAllBytes(filename);
            using (var stream = new MemoryStream(binaryMeshFile))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    var version = reader.ReadInt32();
                    mesh.version = version;
                    var vertexCount = reader.ReadInt32();
                    mesh.vertexCount = vertexCount;
                    var normalsCount = reader.ReadInt32();
                    mesh.normalsCount = normalsCount;
                    var uvCount = reader.ReadInt32();
                    mesh.uvCount = normalsCount;
                    var indicescount = reader.ReadInt32();
                    mesh.indexCount = indicescount;
                    var submeshcount = reader.ReadInt32();
                    mesh.submeshCount = submeshcount;
                    
                    for (int i = 0; i < vertexCount; i++)
                    {
                        mesh.vertices.Add(new Vector3(
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle()
                         ));
                    }
                    for (int i = 0; i < normalsCount; i++)
                    {
                        mesh.normals.Add(new Vector3(
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle()
                         ));
                    }
                    for (int i = 0; i < uvCount; i++)
                    {
                        mesh.uvs.Add(new Vector2(
                            reader.ReadSingle(),
                            reader.ReadSingle()
                         ));
                    }
                    for (int i = 0; i < indicescount; i++)
                    {
                        mesh.indices.Add(reader.ReadInt32());
                    }
                }
            }
            return mesh;
        }

        public static IdentifierData ReadBinaryIdentifiers(string filename)
        {
            IdentifierData identifierdata = new IdentifierData();
            var binarydataFile = File.ReadAllBytes(filename);
            using (var stream = new MemoryStream(binarydataFile))
            {
                using (BinaryReader reader = new BinaryReader(stream,Encoding.UTF8))
                {
                    var version = reader.ReadInt32();
                    identifierdata.version = version;
                    var identifiercount = reader.ReadInt32();
                    identifierdata.identifierCount = identifiercount;
                    for (int i = 0; i < identifiercount; i++)
                    {
                        Identifier identifier = new Identifier();
                        identifier.objectID = reader.ReadString();
                        identifier.startIndex = reader.ReadInt32();
                        identifier.indicesLength = reader.ReadInt32(); 
                        identifier.startVertex = reader.ReadInt32();
                        identifier.vertexLength = reader.ReadInt32();
                        identifier.submeshIndex = reader.ReadInt32();
                        identifierdata.identifiers.Add(identifier);
                    }

                }
            }
            return identifierdata;
        }

       

		private static void Dictionary<T1, T2>()
		{
			throw new NotImplementedException();
		}
	}
}
