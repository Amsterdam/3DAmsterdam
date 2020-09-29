using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUImageRepLoadFile")]
        static extern int SUImageRepLoadFile(
            IntPtr imageRepRef,
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string path);

        public static void ImageRepLoadFile(
            ImageRepRef imageRepRef,
            string path)
        {
            ThrowOut(
                SUImageRepLoadFile(
                    imageRepRef.intPtr,
                    path),
                "Could not load image rep from file.");
        }
    }
}
