using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUFaceGetInnerLoops")]
        static extern int SUFaceGetInnerLoops(
            IntPtr faceRef,
            long len,
            IntPtr[] loopRefs,
            out long count);

        public static void FaceGetInnerLoops(
            FaceRef faceRef,
            long len,
            LoopRef[] loopRefs,
            out long count)
        {
            IntPtr[] intPtrs = new IntPtr[loopRefs.Length];

            ThrowOut(
                SUFaceGetInnerLoops(
                    faceRef.intPtr,
                    len,
                    intPtrs,
                    out count),
                "Could not get inner loops.");

            for (int i = 0; i < loopRefs.Length; ++i)
            {
                loopRefs[i] = new LoopRef();
                loopRefs[i].intPtr = intPtrs[i];
            }
        }
    }
}
