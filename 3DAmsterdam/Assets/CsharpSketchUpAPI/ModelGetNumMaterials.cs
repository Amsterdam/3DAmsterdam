using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUModelGetNumMaterials")]
        static extern int SUModelGetNumMaterials(
            IntPtr modelRef,
            out long numMaterials);

        public static void ModelGetNumMaterials(
            ModelRef modelRef,
            out long numMaterials)
        {
            ThrowOut(
                SUModelGetNumMaterials(
                    modelRef.intPtr,
                    out numMaterials),
                "Could not get number of materials.");
        }
    }
}
