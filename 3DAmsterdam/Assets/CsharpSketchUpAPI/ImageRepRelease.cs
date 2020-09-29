using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUImageRepRelease")]
        static extern int SUImageRepRelease(
            ref IntPtr ImageRepRef);

        public static void ImageRepRelease(
            ImageRepRef imageRepRef)
        {
            ThrowOut(
                SUImageRepRelease(
                    ref imageRepRef.intPtr),
                "Could not release image rep.");
        }
    }
}
