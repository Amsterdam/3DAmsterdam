using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUComponentInstanceSetTransform")]
        static extern int SUComponentInstanceSetTransform(
            IntPtr instanceRef,
            ref Transformation trans);

        public static void ComponentInstanceSetTransform(
            ComponentInstanceRef instanceRef,
            Transformation trans)
        {
            ThrowOut(
                SUComponentInstanceSetTransform(
                    instanceRef.intPtr,
                    ref trans),
                "Could not set instance transform.");
        }
    }
}
