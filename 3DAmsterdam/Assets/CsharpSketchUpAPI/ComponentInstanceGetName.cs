using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUComponentInstanceGetName")]
        static extern int SUComponentInstanceGetName(
            IntPtr instanceRef,
            out IntPtr stringRef);

        public static void ComponentInstanceGetName(
            ComponentInstanceRef instanceRef,
            StringRef stringRef)
        {
            ThrowOut(
                SUComponentInstanceGetName(
                    instanceRef.intPtr,
                    out stringRef.intPtr),
                "Could not get component instance name.");
        }
    }
}
