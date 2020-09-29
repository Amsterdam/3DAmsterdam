using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUModelRelease")]
        static extern int SUModelRelease(
            ref IntPtr modelRef);

        public static void ModelRelease(
            ModelRef modelRef)
        {
            ThrowOut(
                SUModelRelease(
                    ref modelRef.intPtr),
                "Could not release model.");
        }
    }
}
