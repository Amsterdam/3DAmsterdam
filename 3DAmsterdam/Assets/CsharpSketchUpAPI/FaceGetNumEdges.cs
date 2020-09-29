using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUFaceGetNumEdges")]
        static extern int SUFaceGetNumEdges(
            IntPtr faceRef,
            out long numEdges);

        public static void FaceGetNumEdges(
            FaceRef faceRef,
            out long numEdges)
        {
            ThrowOut(
                SUFaceGetNumEdges(
                    faceRef.intPtr,
                    out numEdges),
                "Could not get number of edges.");
        }
    }
}
