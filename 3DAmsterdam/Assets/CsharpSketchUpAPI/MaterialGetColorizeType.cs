using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUMaterialGetColorizeType")]
        static extern int SUMaterialGetColorizeType(
            IntPtr materialRef,
            out int type); // really an enum, not an int

        public static void MaterialGetColorizeType(
            MaterialRef materialRef,
            out int type)
        {
            ThrowOut(
                SUMaterialGetColorizeType(
                    materialRef.intPtr,
                    out type),
                "Could not get material colorize type.");
        }
    }
}
