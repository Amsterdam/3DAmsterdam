using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUFaceGetEdges")]
        static extern int SUFaceGetEdges(
            IntPtr faceRef,
            long len,
            IntPtr[] edgeRefs,
            out long count);

        public static void FaceGetEdges(
            FaceRef faceRef,
            long len,
            EdgeRef[] edgeRefs,
            out long count)
        {
            IntPtr[] intPtrs = new IntPtr[edgeRefs.Length];

            ThrowOut(
                SUFaceGetEdges(
                    faceRef.intPtr,
                    len,
                    intPtrs,
                    out count),
                "Could not get face edges.");

            for (int i = 0; i < edgeRefs.Length; ++i)
            {
                edgeRefs[i] = new EdgeRef();
                edgeRefs[i].intPtr = intPtrs[i];
            }
        }
    }
}
