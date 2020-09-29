using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUImageRepGetRowPadding")]
        static extern int SUImageRepGetRowPadding(
            IntPtr imageRepRef,
            out long padding);

        public static void ImageRepGetRowPadding(
            ImageRepRef imageRepRef,
            out long padding)
        {
            ThrowOut(
                SUImageRepGetRowPadding(
                    imageRepRef.intPtr,
                    out padding),
                "Could not get image rep row padding.");
        }
    }
}
