using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUGeometryInputRelease")]
        static extern int SUGeometryInputRelease(
            ref IntPtr geometryRef);

        public static void GeometryInputRelease(
            GeometryInputRef geometryRef)
        {
            ThrowOut(
                SUGeometryInputRelease(
                    ref geometryRef.intPtr),
                "Could not release geometry.");
        }
    }
}
