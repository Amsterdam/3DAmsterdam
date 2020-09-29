using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUTransformationRotation")]
        static extern int SUTransformationRotation(
            ref Transformation trans,
            ref Point3D point,
            ref Vector3D vector,
            double angle);

        public static void TransformationRotation(
            ref Transformation trans,
            Point3D point,
            Vector3D vector,
            double angle)
        {
            ThrowOut(
                SUTransformationRotation(
                    ref trans,
                    ref point,
                    ref vector,
                    angle),
                "Could not rotate transformation.");
        }
    }
}
