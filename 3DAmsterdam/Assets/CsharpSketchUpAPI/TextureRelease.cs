using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUTextureRelease")]
        static extern int SUTextureRelease(
            ref IntPtr textureRef);

        public static void TextureRelease(
            TextureRef textureRef)
        {
            ThrowOut(
                SUTextureRelease(
                    ref textureRef.intPtr),
                "Could not release texture.");
        }
    }
}
