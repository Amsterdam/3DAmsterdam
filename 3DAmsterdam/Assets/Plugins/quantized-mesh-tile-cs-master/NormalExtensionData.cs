using System.IO;
using System.Runtime.InteropServices;

namespace QuantizedMeshTerrain.Tiles
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NormalExtensionData
    {
        public uint vertexCount;
        public byte[] xy;

        public NormalExtensionData(BinaryReader reader, uint vertCount)
        {
            vertexCount = vertCount;
            xy = new byte[vertexCount * 2];

            for (int i = 0; i < vertexCount * 2; i++)
                xy[i] = reader.ReadByte();
        }
    }
}
