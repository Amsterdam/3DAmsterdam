using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUFaceCreate")]
        static extern int SUFaceCreate(
            out IntPtr faceRef,
            Point3D[] vertices,
            ref IntPtr outerLoop);

        public static void FaceCreate(
            FaceRef faceRef,
            Point3D[] vertices,
            LoopInputRef outerLoop)
        {
            ThrowOut(
                SUFaceCreate(
                    out faceRef.intPtr,
                    vertices,
                    ref outerLoop.intPtr),
                "Could not create face.");
        }
    }
}
