using Terrain.Tiles;
using UnityEngine;

namespace Terrain.ExtensionMethods
{
    public static class TerrainTileExtension
    {
        private const int MAX = 32767;


        public static Vector3[] GetVertices(this TerrainTile t, float hOffset = 0)
        {
            Vector3[] vertices = new Vector3[t.VertexData.vertexCount];
            for (int i = 0; i < t.VertexData.vertexCount; i++)
            {
                //lerp vertices
                var xCoor = t.VertexData.u[i];
                var yCoor = t.VertexData.v[i];
                var height = t.VertexData.height[i];

                var x1 = Terrain.Tiles.Mathf.Lerp(-90, 90, ((double)(xCoor) / MAX));
                var y1 = Terrain.Tiles.Mathf.Lerp(-90, 90, ((double)(yCoor) / MAX));
                var h1 = Terrain.Tiles.Mathf.Lerp(t.Header.MinimumHeight, t.Header.MaximumHeight, ((double)height / MAX));

                vertices[i] = new Vector3((float)x1, (float)h1, (float)y1);
            }
            return vertices;
        }
        public static Vector2[] GetUV(this TerrainTile t, float hOffset = 0)
        {
            Vector2[] uvs = new Vector2[t.VertexData.vertexCount];
            for (int i = 0; i < t.VertexData.vertexCount; i++)
            {
                //lerp vertices
                var xCoor = t.VertexData.u[i];
                var yCoor = t.VertexData.v[i];
                var height = t.VertexData.height[i];

                var x1 = Terrain.Tiles.Mathf.Lerp(-90, 90, ((double)(xCoor) / MAX));
                var y1 = Terrain.Tiles.Mathf.Lerp(-90, 90, ((double)(yCoor) / MAX));
                var h1 = Terrain.Tiles.Mathf.Lerp(t.Header.MinimumHeight, t.Header.MaximumHeight, ((double)height / MAX));

                uvs[i] = new Vector2((float)((x1 + 90) / 180), (float)((y1 + 90) / 180));
            }
            return uvs;
        }

        public static int[] GetTriangles(this TerrainTile t, float hOffset = 0)
        {
            int[] triangles = new int[t.IndexData16.triangleCount * 3];
            for (var i = 0; i < t.IndexData16.indices.Length; i += 3)
            {
                var firstIndex = t.IndexData16.indices[i];
                var secondIndex = t.IndexData16.indices[i + 1];
                var thirdIndex = t.IndexData16.indices[i + 2];

                triangles[i] = firstIndex;
                triangles[i + 1] = thirdIndex;
                triangles[i + 2] = secondIndex;
            }
            return triangles;
        }


       
    }
}
