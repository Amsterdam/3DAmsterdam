using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SULoopInputRelease")]
        static extern int SULoopInputRelease(
            ref IntPtr loopInputRef);

        public static void LoopInputRelease(
            LoopInputRef loopInputRef)
        {
            ThrowOut(
                SULoopInputRelease(
                    ref loopInputRef.intPtr),
                "Could not release loop.");
        }
    }
}
