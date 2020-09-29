using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUModelGetStyles")]
        static extern int SUModelGetStyles(
            IntPtr modelRef,
            out IntPtr stylesRef);

        public static void ModelGetStyles(
            ModelRef modelRef,
            StylesRef stylesRef)
        {
            ThrowOut(
                SUModelGetStyles(
                    modelRef.intPtr,
                    out stylesRef.intPtr),
                "Could not get styles.");
        }
    }
}
