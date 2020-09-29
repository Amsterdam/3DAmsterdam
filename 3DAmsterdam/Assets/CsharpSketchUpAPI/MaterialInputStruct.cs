using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        internal struct MaterialInputStruct
        {
            // size_t num_uv_coords; 

            public long numUVCoords;

            // struct SUPoint2D uv_coords[4]; 

            public double uv0x;
            public double uv0y;

            public double uv1x;
            public double uv1y;

            public double uv2x;
            public double uv2y;

            public double uv3x;
            public double uv3y;

            // size_t vertex_indices[4];

            public long index0;
            public long index1;
            public long index2;
            public long index3;

            // SUMaterialRef material;

            public IntPtr materialRef;
        }
    }
}
