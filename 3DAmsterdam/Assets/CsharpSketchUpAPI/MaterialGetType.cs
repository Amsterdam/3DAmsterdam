using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUMaterialGetType")]
        static extern int SUMaterialGetType(
            IntPtr materialRef,
            out int type); // really an enum, not an int

        public static void MaterialGetType(
            MaterialRef materialRef,
            out int type)
        {
            ThrowOut(
                SUMaterialGetType(
                    materialRef.intPtr,
                    out type),
                "Could not get material type.");
        }
    }
}
