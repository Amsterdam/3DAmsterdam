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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TileBakeLibrary.BinaryMesh;

namespace TileBakeLibrary
{
    class BinaryMeshWriter
    {
        private static int writerVersion = 1;

        public static void WriteMesh(MeshData meshdata, string filename)
        {
            using (FileStream file = File.Create(filename))
            {
                using (BinaryWriter writer = new BinaryWriter(file, Encoding.UTF8))
                {
                    //// write the header

                    // write versionnumber
                    writer.Write(writerVersion);
                    // vertexcount
                    writer.Write(meshdata.vertexCount);
                    writer.Write(meshdata.normalsCount);
                    writer.Write(meshdata.uvCount);
                    writer.Write(meshdata.indexCount);
                    writer.Write(meshdata.submeshCount);

                    // write vertices
                    foreach (Vector3 vertex in meshdata.vertices)
                    {
                        writer.Write(vertex.X);
                        writer.Write(vertex.Y);
                        writer.Write(vertex.Z);
                    }
                    // write normals
                    foreach (Vector3 normal in meshdata.normals)
                    {
                        writer.Write(normal.X);
                        writer.Write(normal.Y);
                        writer.Write(normal.Z);
                    }
                    //write uv's
                    foreach (Vector2 uv in meshdata.uvs)
                    {
                        writer.Write(uv.X);
                        writer.Write(uv.Y);
                    }
                    // write indices
                    foreach (int index in meshdata.indices)
                    {
                        writer.Write(index);
                    }
                    // write submeshes
                    foreach (SubmeshData subMesh in meshdata.submeshes)
                    {
                        writer.Write(subMesh.submeshindex);
                        writer.Write(subMesh.startIndex);
                        writer.Write(subMesh.indexcount);
                        writer.Write(subMesh.startVertex);
                        writer.Write(subMesh.vertexcount);
                    }
                }
            }
        }

        public static void WriteIdentifiers(IdentifierData identifierdata, string filename)
        {
            using (FileStream file = File.Create(filename))
            {
                using (BinaryWriter writer = new BinaryWriter(file, Encoding.UTF8))
                {
                    // write versionnumber
                    writer.Write(writerVersion);
                    // identifiercount
                    writer.Write(identifierdata.identifiers.Count);
                    foreach (Identifier identifier in identifierdata.identifiers)
                    {
                        writer.Write(identifier.objectID);
                        writer.Write(identifier.startIndex);
                        writer.Write(identifier.indicesLength);
                        writer.Write(identifier.startVertex);
                        writer.Write(identifier.vertexLength);
                        writer.Write(identifier.submeshIndex);
                    }
                }
            }
        }


    }
        
}
