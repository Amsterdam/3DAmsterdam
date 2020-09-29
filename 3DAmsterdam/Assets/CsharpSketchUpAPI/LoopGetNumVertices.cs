using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SULoopGetNumVertices")]
        static extern int SULoopGetNumVertices(
            IntPtr loopRef,
            out long numVertices);

        public static void LoopGetNumVertices(
            LoopRef loopRef,
            out long numVertices)
        {
            ThrowOut(
                SULoopGetNumVertices(
                    loopRef.intPtr,
                    out numVertices),
                "Could not get number of vertices.");
        }
    }
}
