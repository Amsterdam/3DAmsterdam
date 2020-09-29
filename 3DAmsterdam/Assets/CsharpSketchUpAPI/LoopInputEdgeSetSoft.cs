using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SULoopInputEdgeSetSoft")]
        static extern int SULoopInputEdgeSetSoft(
            IntPtr loopInputRef,
            long index,
            bool soft);

        public static void LoopInputEdgeSetSoft(
            LoopInputRef loopInputRef,
            long index, 
            bool soft)
        {
            ThrowOut(
                SULoopInputEdgeSetSoft(
                    loopInputRef.intPtr,
                    index,
                    soft),
                "Could not set edge soft.");
        }
    }
}
