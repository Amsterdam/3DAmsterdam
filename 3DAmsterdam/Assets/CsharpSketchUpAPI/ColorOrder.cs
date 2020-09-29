using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ColorOrder
        {
            public short redIndex;
            public short greenIndex;
            public short blueIndex;
            public short alphaIndex;
        }
    }
}
