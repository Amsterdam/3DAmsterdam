using System.IO;
using System.Runtime.InteropServices;

namespace QuantizedMeshTerrain.Tiles
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ExtensionHeader
    {
        public byte extensionId;
        public uint extensionLength;

        public ExtensionHeader(BinaryReader reader)
        {
            extensionId = reader.ReadByte();
            extensionLength = reader.ReadUInt32();
        }
    }
}
