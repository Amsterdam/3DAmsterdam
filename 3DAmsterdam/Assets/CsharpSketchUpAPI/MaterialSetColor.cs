using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUMaterialSetColor")]
        static extern int SUMaterialSetColor(
            IntPtr materialRef,
            ref Color color);

        public static void MaterialSetColor(
            MaterialRef materialRef,
            Color color)
        {
            ThrowOut(
                SUMaterialSetColor(
                    materialRef.intPtr,
                    ref color),
                "Could not set color.");
        }
    }
}
