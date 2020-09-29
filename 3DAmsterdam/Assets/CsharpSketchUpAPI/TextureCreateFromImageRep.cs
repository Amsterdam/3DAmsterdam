using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUTextureCreateFromImageRep")]
        static extern int SUTextureCreateFromImageRep(
            out IntPtr textureRef,
            IntPtr imageRepRef);

        public static void TextureCreateFromImageRep(
            TextureRef textureRef,
            ImageRepRef imageRepRef)
        {
            ThrowOut(
                SUTextureCreateFromImageRep(
                    out textureRef.intPtr,
                    imageRepRef.intPtr),
                "Could not create texture from image rep.");
        }
    }
}
