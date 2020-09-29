using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUFaceGetNumInnerLoops")]
        static extern int SUFaceGetNumInnerLoops(
            IntPtr faceRef,
            out long count);

        public static void FaceGetNumInnerLoops(
            FaceRef faceRef,
            out long count)
        {
            ThrowOut(
                SUFaceGetNumInnerLoops(
                    faceRef.intPtr,
                    out count),
                "Could not get number of inner loops.");
        }
    }
}
