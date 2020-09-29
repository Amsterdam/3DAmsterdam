using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUMaterialCreate")]
        static extern int SUMaterialCreate(
            out IntPtr materialRef);

        public static void MaterialCreate(
            MaterialRef materialRef)
        {
            ThrowOut(
                SUMaterialCreate(
                    out materialRef.intPtr),
                "Could not create material.");
        }
    }
}
