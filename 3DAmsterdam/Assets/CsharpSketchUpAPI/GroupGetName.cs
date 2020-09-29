using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUGroupGetName")]
        static extern int SUGroupGetName(
            IntPtr groupRef,
            out IntPtr stringRef);

        public static void GroupGetName(
            GroupRef groupRef,
            StringRef stringRef)
        {
            ThrowOut(
                SUGroupGetName(
                    groupRef.intPtr,
                    out stringRef.intPtr),
                "Could not get group name.");
        }
    }
}
