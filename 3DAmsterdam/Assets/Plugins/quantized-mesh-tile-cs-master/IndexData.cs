using System.IO;
using System.Runtime.InteropServices;

namespace QuantizedMeshTerrain.Tiles
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IndexData16
    {
        public uint triangleCount;
        public ushort[] indices;

        public IndexData16(BinaryReader reader)
        {
            triangleCount = reader.ReadUInt32();
            indices = new ushort[triangleCount * 3];

            ushort highest = 0;
            for (int i = 0; i < indices.Length; i++)
            {
                ushort code = reader.ReadUInt16();
                indices[i] = (ushort)(highest - code);

                if (code == 0)
                    highest++;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IndexData32
    {
        public uint triangleCount;
        public uint[] indices;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EdgeIndices16
    {
        public uint westVertexCount;
        public ushort[] westIndices;

        public uint southVertexCount;
        public ushort[] southIndices;

        public uint eastVertexCount;
        public ushort[] eastIndices;

        public uint northVertexCount;
        public ushort[] northIndices;

        public EdgeIndices16(BinaryReader reader)
        {
            westVertexCount = reader.ReadUInt32();
            westIndices = new ushort[westVertexCount];

            for (int i = 0; i < westVertexCount; i++)
                westIndices[i] = reader.ReadUInt16();

            southVertexCount = reader.ReadUInt32();
            southIndices = new ushort[southVertexCount];

            for (int i = 0; i < southVertexCount; i++)
                southIndices[i] = reader.ReadUInt16();

            eastVertexCount = reader.ReadUInt32();
            eastIndices = new ushort[eastVertexCount];

            for (int i = 0; i < eastVertexCount; i++)
                eastIndices[i] = reader.ReadUInt16();

            northVertexCount = reader.ReadUInt32();
            northIndices = new ushort[northVertexCount];

            for (int i = 0; i < northVertexCount; i++)
                northIndices[i] = reader.ReadUInt16();
        }
    }
}
