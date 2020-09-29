using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUGroupGetTransform")]
        static extern int SUGroupGetTransform(
            IntPtr groupRef,
            out Transformation trans);

        public static void GroupGetTransform(
            GroupRef groupRef,
            out Transformation trans)
        {
            ThrowOut(
                SUGroupGetTransform(
                    groupRef.intPtr,
                    out trans),
                "Could not get group transform.");
        }
    }
}
