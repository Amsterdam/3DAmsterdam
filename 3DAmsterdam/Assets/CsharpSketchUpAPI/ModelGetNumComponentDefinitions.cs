using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUModelGetNumComponentDefinitions")]
        static extern int SUModelGetNumComponentDefinitions(
            IntPtr modelRef,
            out long numDefinitions);

        public static void ModelGetNumComponentDefinitions(
            ModelRef modelRef,
            out long numDefinitions)
        {
            ThrowOut(
                SUModelGetNumComponentDefinitions(
                    modelRef.intPtr,
                    out numDefinitions),
                "Could not get number of definitions.");
        }
    }
}
