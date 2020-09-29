using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUMaterialGetTexture")]
        static extern int SUMaterialGetTexture(
            IntPtr materialRef,
            out IntPtr textureRef);

        public static void MaterialGetTexture(
            MaterialRef materialRef,
            TextureRef textureRef)
        {
            ThrowOut(
                SUMaterialGetTexture(
                    materialRef.intPtr,
                    out textureRef.intPtr),
                "Could not get texture.");
        }
    }
}
