using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUMaterialGetColor")]
        static extern int SUMaterialGetColor(
            IntPtr materialRef,
            out Color color);

        public static void MaterialGetColor(
            MaterialRef materialRef,
            out Color color)
        {
            ThrowOut(
                SUMaterialGetColor(
                    materialRef.intPtr,
                    out color),
                "Could not get color.");
        }
    }
}
