using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUModelCreateFromFile")]
        static extern int SUModelCreateFromFile(
            out IntPtr modelRef,
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string name);

        public static void ModelCreateFromFile(
            ModelRef modelRef,
            string name)
        {
            ThrowOut(
                SUModelCreateFromFile(
                    out modelRef.intPtr,
                    name),
                "Could not create model from file.");
        }
    }
}
