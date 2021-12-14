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
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TileBakeLibrary.Coordinates;
using TileBakeLibrary;
namespace TileBakeLibrary.BinaryMesh
{
    class BinaryMeshData
    {
        public  MeshData mesh = new MeshData();
        public  IdentifierData semantics = new IdentifierData();

        private Vector2Double origin;
        private Vector2 tileSize;
        private List<Submesh> submeshes = new List<Submesh>();
        /// <summary>
        /// export data from a tile to a binaryMesh an a binary data mesh
        /// </summary>
        /// <param name="tile"></param>
        public void ExportData(Tile tile)
        {
            origin = tile.position;
            tileSize = tile.size;

            // move the subobjects to the correct submeshes
            List<SubObject> subobjects = tile.SubObjects;
            foreach (SubObject subobject in subobjects)
            {
                int submeshindex = subobject.parentSubmeshIndex;
                ExpandSubmeshList(submeshindex);
                submeshes[submeshindex].subobjects.Add(subobject);
            }

            int startindex = 0;
            int startvertex = 0;
            foreach (Submesh subMesh in submeshes)
            {
                //skip the submesh if there are no subobjects in it
                if (subMesh.subobjects.Count==0)
                {
                    continue;
                }
                SubmeshData submeshData = new SubmeshData();
                submeshData.submeshindex = subMesh.index;
                submeshData.startIndex = startindex;
                submeshData.startVertex = startvertex;
                foreach (SubObject subobject in subMesh.subobjects)
                {
                    Identifier identifier = new Identifier();
                    identifier.submeshIndex = subMesh.index;
                    identifier.startIndex = startindex;
                    identifier.startVertex = startvertex;
                    identifier.objectID = subobject.id;
                    identifier.vertexLength = subobject.vertices.Count;
                    identifier.indicesLength = subobject.triangleIndices.Count;
                    semantics.identifiers.Add(identifier);

                    // add vertices and normals to the mesh
                    for (int i = 0; i < subobject.vertices.Count; i++)
                    {
                        mesh.vertices.Add(ConvertVertex(subobject.vertices[i]));
                        mesh.normals.Add(ConvertNormaltoBinary(subobject.normals[i]));
                    }
                    
                    // offset the indices with the value of startvertex and add to the mesh
                    for (int i = 0; i < subobject.triangleIndices.Count; i++)
                    {
                        mesh.indices.Add(subobject.triangleIndices[i] + startvertex);
                    }
                    //update startvertex
                    startvertex = mesh.vertices.Count;
                    //update startindex;
                    startindex = mesh.indices.Count;
                    
                }
                submeshData.indexcount = startindex - submeshData.startIndex;
                submeshData.vertexcount = startvertex - submeshData.startVertex;
                mesh.submeshes.Add(submeshData);
                mesh.submeshCount = mesh.submeshes.Count;
            }
            mesh.vertexCount = mesh.vertices.Count;
            mesh.normalsCount = mesh.normals.Count;
            mesh.indexCount = mesh.indices.Count;


            //write bin-file
            BinaryMeshWriter.WriteMesh(mesh, tile.filePath);

            // write data-file
            string datafilepath = tile.filePath.Replace(".bin", "-data.bin");
            BinaryMeshWriter.WriteIdentifiers(semantics, datafilepath);

            //write gltf-wrapper
            string gltfFilename = tile.filePath.Replace(".bin", ".gltf");
            GltfWrapper.Save(mesh, gltfFilename, tile.filePath);

        }
        private Vector3 ConvertNormaltoBinary(Vector3 normal)
        {
            return new Vector3(normal.X, normal.Z, normal.Y);
        }
        private Vector3 ConvertNormalFormBinary(Vector3 normal)
        {
            return new Vector3(normal.X, normal.Z, normal.Y);
        }

        private Vector3 ConvertVertex(Vector3Double vertex)
        {
            double X = vertex.X - origin.X - (tileSize.X/2);
            double Y = vertex.Z;
            double Z = vertex.Y - origin.Y - (tileSize.Y/2);
            return new Vector3((float)X, (float)Y, (float)Z);
        }

        private Vector3Double ConvertVertex(Vector3 vertex)
        {
            double X = vertex.X + origin.X + (tileSize.X/2);
            double Y = vertex.Z + origin.Y + (tileSize.Y/2);
            double Z = vertex.Y;
            return new Vector3Double(X, Y, Z);
        }

        public void ImportData(Tile tile)
        {
            origin = tile.position;
            tileSize = tile.size;
            MeshData mesh = BinaryMeshReader.ReadBinaryMesh(tile.filePath);
            string identifierFilename = tile.filePath.Replace(".bin", "-data.bin");
            IdentifierData identifierdata = BinaryMeshReader.ReadBinaryIdentifiers(identifierFilename);
            foreach (Identifier identifier in identifierdata.identifiers)
            {
                SubObject subobject = new SubObject();
                subobject.id = identifier.objectID;
                subobject.parentSubmeshIndex = identifier.submeshIndex;
                // get normals and vertices
                for (int i = 0; i < identifier.vertexLength; i++)
                {
                    subobject.vertices.Add(ConvertVertex(mesh.vertices[i + identifier.startVertex]));
                    subobject.normals.Add(ConvertNormalFormBinary(mesh.normals[i + identifier.startVertex]));
                }
                // get indices
                for (int i = 0; i < identifier.indicesLength; i++)
                {
                    subobject.triangleIndices.Add(mesh.indices[i + identifier.startIndex]-identifier.startVertex);
                }
                tile.AddSubObject(subobject, false);
            }
        }

        private void ExpandSubmeshList(int submeshIndex)
        {
            int addAmount = submeshIndex+1 - submeshes.Count;
            for (int i = 0; i < addAmount; i++)
            {
                Submesh newSubmesh = new Submesh();
                newSubmesh.index = submeshes.Count;
                submeshes.Add(newSubmesh);
            }
        }
    }

    public class MeshData
    {
        public int version;

        public int vertexCount;
        public List<Vector3> vertices = new List<Vector3>();
        public int normalsCount;
        public List<Vector3> normals = new List<Vector3>();
        public int uvCount;
        public List<Vector2> uvs = new List<Vector2>();

        public List<int> indices = new List<int>();
        public int indexCount;
        public int submeshCount;

        public List<SubmeshData> submeshes = new List<SubmeshData>();
    }
    public class SubmeshData
    {
        public int submeshindex;
        public int startIndex;
        public int startVertex;
        public int indexcount;
        public int vertexcount;
    }
    public class IdentifierData
    {
        public int version;
        public int identifierCount;
        public List<Identifier> identifiers = new List<Identifier>();
    }

    public class Submesh
    {
        public int index;
        public List<SubObject> subobjects = new List<SubObject>();
    }

    public class Identifier
    {
        public string objectID;
        public int indicesLength;
        public int startIndex;
        public int vertexLength;
        public int startVertex;
        public int submeshIndex;
    }
}
