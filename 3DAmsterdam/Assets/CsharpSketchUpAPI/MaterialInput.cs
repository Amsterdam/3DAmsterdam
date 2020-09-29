namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        public class MaterialInput
        {
            public int numUVCoords;

            public Point2D[] UVCoords = new Point2D[4];
            public int[] vertexIndices = new int[4];

            public MaterialRef materialRef;

            public void SetUVCoords(params Point2D[] coords)
            {
                for (int c = 0; c < System.Math.Min(4, coords.Length); ++c)
                {
                    UVCoords[c] = coords[c];
                }
            }

            public void SetVertexIndices(params int[] indices)
            {
                for (int v = 0; v < System.Math.Min(4, indices.Length); ++v)
                {
                    vertexIndices[v] = indices[v];
                }
            }

            internal MaterialInputStruct materialInputStruct()
            {
                MaterialInputStruct s = new MaterialInputStruct
                {
                    numUVCoords = numUVCoords,

                    uv0x = UVCoords[0].x,
                    uv0y = UVCoords[0].y,

                    uv1x = UVCoords[1].x,
                    uv1y = UVCoords[1].y,

                    uv2x = UVCoords[2].x,
                    uv2y = UVCoords[2].y,

                    uv3x = UVCoords[3].x,
                    uv3y = UVCoords[3].y,

                    index0 = vertexIndices[0],
                    index1 = vertexIndices[1],
                    index2 = vertexIndices[2],
                    index3 = vertexIndices[3],

                    materialRef = materialRef.intPtr
                };

                return s;
            }
        }
    }
}
