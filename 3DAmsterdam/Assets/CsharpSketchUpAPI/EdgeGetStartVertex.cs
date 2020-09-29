using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUEdgeGetStartVertex")]
        static extern int EdgeGetStartVertex(
            IntPtr edgeRef,
            out IntPtr vertexRef);

        public static void EdgeGetStartVertex(
            EdgeRef edgeRef,
            VertexRef vertexRef)
        {
            ThrowOut(
                EdgeGetStartVertex(
                    edgeRef.intPtr,
                    out vertexRef.intPtr),
                "Could not get start vertex.");
        }
    }
}
