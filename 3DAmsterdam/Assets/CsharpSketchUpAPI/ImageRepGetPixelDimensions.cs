using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUImageRepGetPixelDimensions")]
        static extern int SUImageRepGetPixelDimensions(
            IntPtr imageRepRef,
            out long width,
            out long height);

        public static void ImageRepGetPixelDimensions(
            ImageRepRef imageRepRef,
            out long width,
            out long height)
        {
            ThrowOut(
                SUImageRepGetPixelDimensions(
                    imageRepRef.intPtr,
                    out width,
                    out height),
                "Could not get image rep pixel dimensions.");
        }
    }
}
