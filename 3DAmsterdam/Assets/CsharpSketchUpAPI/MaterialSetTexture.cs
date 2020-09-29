using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUMaterialSetTexture")]
        static extern int SUMaterialSetTexture(
            IntPtr materialRef,
            IntPtr textureRef);

        public static void MaterialSetTexture(
            MaterialRef materialRef,
            TextureRef textureRef)
        {
            ThrowOut(
                SUMaterialSetTexture(
                    materialRef.intPtr,
                    textureRef.intPtr),
                "Could not set texture.");
        }
    }
}
