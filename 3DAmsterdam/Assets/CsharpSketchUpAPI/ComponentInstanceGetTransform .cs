using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUComponentInstanceGetTransform")]
        static extern int SUComponentInstanceGetTransform(
            IntPtr instanceRef,
            out Transformation trans);

        public static void ComponentInstanceGetTransform(
            ComponentInstanceRef instanceRef,
            out Transformation trans)
        {
            ThrowOut(
                SUComponentInstanceGetTransform(
                    instanceRef.intPtr,
                    out trans),
                "Could not get instance transform.");
        }
    }
}
