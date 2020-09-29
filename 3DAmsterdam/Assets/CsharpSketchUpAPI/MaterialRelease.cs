using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUMaterialRelease")]
        static extern int SUMaterialRelease(
            ref IntPtr materialRef);

        public static void MaterialRelease(
            MaterialRef materialRef)
        {
            ThrowOut(
                SUMaterialRelease(
                    ref materialRef.intPtr),
                "Could not release material");
        }
    }
}
