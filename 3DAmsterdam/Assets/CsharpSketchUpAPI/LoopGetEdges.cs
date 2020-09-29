using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SULoopGetEdges")]
        static extern int SULoopGetEdges(
            IntPtr loopRef,
            long len,
            IntPtr[] edgeRefs,
            out long count);

        public static void LoopGetEdges(
            LoopRef loopRef,
            long len,
            EdgeRef[] edgeRefs,
            out long count)
        {
            IntPtr[] intPtrs = new IntPtr[edgeRefs.Length];

            ThrowOut(
                SULoopGetEdges(
                    loopRef.intPtr,
                    len,
                    intPtrs,
                    out count),
                "Could not get loop edges.");

            for (int i = 0; i < edgeRefs.Length; ++i)
            {
                edgeRefs[i] = new EdgeRef();
                edgeRefs[i].intPtr = intPtrs[i];
            }
        }
    }
}
