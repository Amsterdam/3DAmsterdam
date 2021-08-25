using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Netherlands3D.ModelParsing
{
    public struct Vertex 
    {
        public Vector3 coordinate;
        public Dictionary<Vector3, int> normals;

    }
    public struct VertexNormal
    {
        public Vector3 normal;
        public int finalIndex;
    }

    struct Submesh
    {
        public Dictionary<Vector3,Vertex> vertices;
        public List<int> indices;
        public string name;
        public int startIndex;
        public int indexCount;
        public int vertexCount;
        public int startVertex;

    }
}
public class OBJMeshBuilder : MonoBehaviour
{
    
}
