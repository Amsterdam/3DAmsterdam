using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUGeometryInputAddFace")]
        static extern int SUGeometryInputAddFace(
            IntPtr geometryInputRef,
            ref IntPtr loopInputRef,
            out long index);

        public static void GeometryInputAddFace(
            GeometryInputRef geometryInputRef,
            LoopInputRef loopInputRef,
            out long index)
        {
            ThrowOut(
                SUGeometryInputAddFace(
                    geometryInputRef.intPtr,
                    ref loopInputRef.intPtr, 
                    out index),
                "Could not add face.");
        }
    }
}
