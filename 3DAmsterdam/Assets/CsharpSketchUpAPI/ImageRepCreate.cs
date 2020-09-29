using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUImageRepCreate")]
        static extern int SUImageRepCreate(
            out IntPtr imageRepRef);

        public static void ImageRepCreate(
            ImageRepRef imageRepRef)
        {
            ThrowOut(
                SUImageRepCreate(
                    out imageRepRef.intPtr),
                "Could not create image rep.");
        }
    }
}
