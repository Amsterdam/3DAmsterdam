using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUEdgeGetSmooth")]
        static extern int SUEdgeGetSmooth(
            IntPtr edgeRef,
            out bool smooth);

        public static void EdgeGetSmooth(
            EdgeRef edgeRef,
            out bool smooth)
        {
            ThrowOut(
                SUEdgeGetSmooth(
                    edgeRef.intPtr,
                    out smooth),
                "Could not get edge smooth.");
        }
    }
}
