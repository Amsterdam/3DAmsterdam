using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUModelCreate")]
        static extern int SUModelCreate(
            out IntPtr modelRef);

        public static void ModelCreate(
            ModelRef modelRef)
        {
            ThrowOut(
                SUModelCreate(
                    out modelRef.intPtr),
                "Could not create model.");
        }
    }
}
