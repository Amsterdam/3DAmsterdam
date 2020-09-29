using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUUVHelperRelease")]
        static extern int SUUVHelperRelease(
            ref IntPtr uvHelperRef);

        public static void UVHelperRelease(
            UVHelperRef uvHelperRef)
        {
            ThrowOut(
                SUUVHelperRelease(
                    ref uvHelperRef.intPtr),
                "Could not release UVHelper");
        }
    }
}
