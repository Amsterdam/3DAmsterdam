using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUTextureGetImageRep")]
        static extern int SUTextureGetImageRep(
            IntPtr textureRef,
            out IntPtr imageRepRef);

        public static void TextureGetImageRep(
            TextureRef textureRef,
            ImageRepRef imageRepRef)
        {
            ThrowOut(
                SUTextureGetImageRep(
                    textureRef.intPtr,
                    out imageRepRef.intPtr),
                "Could not get image rep.");
        }
    }
}
