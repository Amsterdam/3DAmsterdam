using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUImageRepGetDataSize")]
        static extern int SUImageRepGetDataSize(
            IntPtr imageRepRef,
            out long dataSize,
            out long bitsPerPixel);

        public static void ImageRepGetDataSize(
            ImageRepRef imageRepRef,
            out long dataSize,
            out long bitsPerPixel)
        {
            ThrowOut(
                SUImageRepGetDataSize(
                    imageRepRef.intPtr,
                    out dataSize,
                    out bitsPerPixel),
                "Could not image rep data size.");
        }
    }
}
