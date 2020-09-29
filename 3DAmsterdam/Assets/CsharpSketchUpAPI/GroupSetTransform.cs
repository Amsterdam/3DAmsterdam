using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUGroupSetTransform")]
        static extern int SUGroupSetTransform(
            IntPtr groupRef,
            ref Transformation trans);

        public static void GroupSetTransform(
            GroupRef groupRef,
            Transformation trans)
        {
            ThrowOut(
                SUGroupSetTransform(
                    groupRef.intPtr,
                    ref trans),
                "Could not set group transform.");
        }
    }
}
