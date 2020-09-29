using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUUVHelperGetFrontUVQ")]
        static extern int UVHelperGetFrontUVQ(
            IntPtr uvHelperRef,
            Point3D point,
            out UVQ uvq);

        public static void UVHelperGetFrontUVQ(
            UVHelperRef uvHelperRef,
            Point3D point,
            out UVQ uvq)
        {
            ThrowOut(
                UVHelperGetFrontUVQ(
                    uvHelperRef.intPtr,
                    point,
                    out uvq),
                "Could not get front UVQ.");
        }
    }
}
