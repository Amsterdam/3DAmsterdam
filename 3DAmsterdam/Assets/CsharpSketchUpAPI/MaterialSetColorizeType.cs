using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUMaterialSetColorizeType")]
        static extern int SUMaterialSetColorizeType(
            IntPtr materialRef,
            int colorizeType);

        public static void MaterialSetColorizeType(
            MaterialRef materialRef,
            int colorizeType)
        {
            ThrowOut(
                SUMaterialSetColorizeType(
                    materialRef.intPtr,
                    colorizeType),
                "Could not set colorize type.");
        }
    }
}
