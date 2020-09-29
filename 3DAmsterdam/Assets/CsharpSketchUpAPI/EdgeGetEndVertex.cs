using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUEdgeGetEndVertex")]
        static extern int EdgeGetEndVertex(
            IntPtr edgeRef,
            out IntPtr vertexRef);

        public static void EdgeGetEndVertex(
            EdgeRef edgeRef,
            VertexRef vertexRef)
        {
            ThrowOut(
                EdgeGetEndVertex(
                    edgeRef.intPtr,
                    out vertexRef.intPtr),
                "Could not get end vertex.");
        }
    }
}
