using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SULoopInputEdgeSetSmooth")]
        static extern int SULoopInputEdgeSetSmooth(
            IntPtr loopInputRef,
            long index,
            bool smooth);

        public static void LoopInputEdgeSetSmooth(
            LoopInputRef loopInputRef,
            long index,
            bool smooth)
        {
            ThrowOut(
                SULoopInputEdgeSetSmooth(
                    loopInputRef.intPtr,
                    index, smooth),
                "Could not set edge smooth.");
        }
    }
}
