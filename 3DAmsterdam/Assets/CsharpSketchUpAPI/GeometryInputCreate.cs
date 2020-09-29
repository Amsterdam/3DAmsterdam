using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUGeometryInputCreate")]
        static extern int SUGeometryInputCreate(
            out IntPtr geometryInputRef);

        public static void GeometryInputCreate(
            GeometryInputRef geometryInputRef)
        {
            ThrowOut(
                SUGeometryInputCreate(
                    out geometryInputRef.intPtr),
                "Could not create geometry input.");
        }
    }
}
