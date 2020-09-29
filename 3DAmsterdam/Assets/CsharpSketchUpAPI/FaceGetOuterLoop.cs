using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUFaceGetOuterLoop")]
        static extern int FaceGetOuterLoop(
            IntPtr faceRef,
            out IntPtr loopRef);

        public static void FaceGetOuterLoop(
            FaceRef faceRef,
            LoopRef loopRef)
        {
            ThrowOut(
                FaceGetOuterLoop(
                    faceRef.intPtr,
                    out loopRef.intPtr),
                "Could not get outer loop.");
        }
    }
}
