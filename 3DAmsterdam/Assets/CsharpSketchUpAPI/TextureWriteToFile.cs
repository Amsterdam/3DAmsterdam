using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUTextureWriteToFile")]
        static extern int SUTextureWriteToFile(
            IntPtr textureRef,
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string path);

        public static void TextureWriteToFile(
            TextureRef textureRef,
            string path)
        {
            ThrowOut(
                SUTextureWriteToFile(
                    textureRef.intPtr,
                    path),
                "Could not write texture to file.");
        }
    }
}
