using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUFaceGetUVHelper")]
        static extern int SUFaceGetUVHelper(
            IntPtr faceRef,
            bool front,
            bool back,
            IntPtr textureWriterRef,
            out IntPtr uvHelperRef);

        public static void FaceGetUVHelper(
            FaceRef faceRef,
            bool front,
            bool back,
            TextureWriterRef textureWriterRef,
            UVHelperRef uvHelperRef)
        {
            ThrowOut(
                SUFaceGetUVHelper(
                    faceRef.intPtr,
                    front,
                    back,
                    textureWriterRef.intPtr,
                    out uvHelperRef.intPtr),
                "Could not get UVHelper.");
        }
    }
}
