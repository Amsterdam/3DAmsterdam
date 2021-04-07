using System;
using System.IO;
using System.Runtime.InteropServices;

namespace QuantizedMeshTerrain.Tiles
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexData
    {
        public uint vertexCount;
        public ushort[] u;
        public ushort[] v;
        public ushort[] height;

        public VertexData(BinaryReader reader)
        {
            vertexCount = reader.ReadUInt32();
            u = new ushort[vertexCount];
            v = new ushort[vertexCount];
            height = new ushort[vertexCount];

            if (vertexCount > 64 * 1024)
                throw new NotSupportedException("32 bit indices not supported yet");

            for (int i = 0; i < vertexCount; i++)
                u[i] = reader.ReadUInt16();

            for (int i = 0; i < vertexCount; i++)
                v[i] = reader.ReadUInt16();

            for (int i = 0; i < vertexCount; i++)
                height[i] = reader.ReadUInt16();

            ushort _u = 0;
            ushort _v = 0;
            ushort _height = 0;

            for (int i = 0; i < vertexCount; i++)
            {
                _u += (ushort)ZigZag.Decode(u[i]);
                _v += (ushort)ZigZag.Decode(v[i]);
                _height += (ushort)ZigZag.Decode(height[i]);

                u[i] = _u;
                v[i] = _v;
                height[i] = _height;
            }
        }

    }
}
