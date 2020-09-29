using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUMaterialGetNameLegacyBehavior")]
        static extern int SUMaterialGetNameLegacyBehavior(
            IntPtr materialRef,
            out IntPtr stringRef);

        public static void MaterialGetNameLegacyBehavior(
            MaterialRef materialRef,
            StringRef stringRef)
        {
            ThrowOut(
                SUMaterialGetNameLegacyBehavior(
                    materialRef.intPtr,
                    out stringRef.intPtr),
                "Could not get material legacy name.");
        }
    }
}
