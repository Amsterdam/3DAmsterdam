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
            double MinHeight = t.Header.MinimumHeight;
            double MaxHeight = t.Header.MaximumHeight;
            VertexData verts = t.VertexData;
            for (int i = 0; i < verts.vertexCount; i++)
            {
                //lerp vertices
                var xCoor = verts.u[i];
                var yCoor = verts.v[i];
                var height = verts.height[i];

                var x1 = Terrain.Tiles.Mathf.Lerp(-90, 90, ((double)(xCoor) / MAX));
                var y1 = Terrain.Tiles.Mathf.Lerp(-90, 90, ((double)(yCoor) / MAX));
                var h1 = Terrain.Tiles.Mathf.Lerp(MinHeight, MaxHeight, ((double)height / MAX));

                vertices[i] = new Vector3((float)x1, (float)h1, (float)y1);
            }
            return vertices;
        }
        public static Vector2[] GetUV(this TerrainTile t, float hOffset = 0)
        {
            double MinHeight = t.Header.MinimumHeight;
            double MaxHeight = t.Header.MaximumHeight;
            Vector2[] uvs = new Vector2[t.VertexData.vertexCount];
            VertexData vertData = t.VertexData;

            for (int i = 0; i < vertData.vertexCount; i++)
            {
                //lerp vertices
                var xCoor = vertData.u[i];
                var yCoor = vertData.v[i];
                var height = vertData.height[i];

                var x1 = Terrain.Tiles.Mathf.Lerp(-90, 90, ((double)(xCoor) / MAX));
                var y1 = Terrain.Tiles.Mathf.Lerp(-90, 90, ((double)(yCoor) / MAX));
                var h1 = Terrain.Tiles.Mathf.Lerp(MinHeight, MaxHeight, ((double)height / MAX));

                uvs[i] = new Vector2((float)((x1 + 90) / 180), (float)((y1 + 90) / 180));
            }
            return uvs;
        }

        public static int[] GetTriangles(this TerrainTile t, float hOffset = 0)
        {


            int[] triangles = new int[t.IndexData16.triangleCount * 3];
            ushort[] indexes = t.IndexData16.indices;
            for (var i = 0; i < indexes.Length; i += 3)
            {
                var firstIndex = indexes[i];
                var secondIndex = indexes[i + 1];
                var thirdIndex = indexes[i + 2];

                triangles[i] = firstIndex;
                triangles[i + 1] = thirdIndex;
                triangles[i + 2] = secondIndex;
            }
            return triangles;
        }


       
    }
}
