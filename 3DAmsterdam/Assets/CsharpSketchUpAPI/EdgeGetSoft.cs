using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUEdgeGetSoft")]
        static extern int SUEdgeGetSoft(
            IntPtr edgeRef,
            out bool soft);

        public static void EdgeGetSoft(
            EdgeRef edgeRef,
            out bool soft)
        {
            ThrowOut(
                SUEdgeGetSoft(
                    edgeRef.intPtr,
                    out soft),
                "Could not get edge soft.");
        }
    }
}
