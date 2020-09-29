using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUTextureGetColorizedImageRep")]
        static extern int SUTextureGetColorizedImageRep(
            IntPtr textureRef,
            out IntPtr imageRepRef);

        public static void TextureGetColorizedImageRep(
            TextureRef textureRef,
            ImageRepRef imageRepRef)
        {
            ThrowOut(
                SUTextureGetColorizedImageRep(
                    textureRef.intPtr,
                    out imageRepRef.intPtr),
                "Could not get colorized image rep.");
        }
    }
}
