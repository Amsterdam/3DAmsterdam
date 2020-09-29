using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SULoopInputAddVertexIndex")]
        static extern int SULoopInputAddVertexIndex(
            IntPtr loopInputRef,
            long index);

        public static void LoopInputAddVertexIndex(
            LoopInputRef loopInputRef,
            long index)
        {
            ThrowOut(
                SULoopInputAddVertexIndex(
                    loopInputRef.intPtr,
                    index),
                "Could not add vertex index.");
        }
    }
}
