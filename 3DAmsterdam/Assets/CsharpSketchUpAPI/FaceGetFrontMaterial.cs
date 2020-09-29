using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUFaceGetFrontMaterial")]
        static extern int SUFaceGetFrontMaterial(
            IntPtr faceRef,
            out IntPtr materialRef);

        public static void FaceGetFrontMaterial(
            FaceRef faceRef,
            MaterialRef materialRef)
        {
            ThrowOut(
                SUFaceGetFrontMaterial(
                    faceRef.intPtr,
                    out materialRef.intPtr),
                    "Could not get face front material");
        }
    }

}
