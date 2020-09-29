using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUTransformationNonUniformScale")]
        static extern int SUTransformationNonUniformScale(
            ref Transformation trans,
            double xScale,
            double yScale,
            double zScale);

        public static void TransformationNonUniformScale(
            ref Transformation trans,
            double xScale,
            double yScale,
            double zScale)
        {
            ThrowOut(
                SUTransformationNonUniformScale(
                    ref trans,
                    xScale,
                    yScale,
                    zScale),
                "Could not scale transformation.");
        }
    }
}
