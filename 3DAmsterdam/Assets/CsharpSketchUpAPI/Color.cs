using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct Color
        {
            public byte red;
            public byte green;
            public byte blue;
            public byte alpha;
        }
    }
}
