using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUTransformationTranslation")]
        static extern int SUTransformationTranslation(
            ref Transformation trans,
            ref Vector3D vector);

        public static void TransformationTranslation(
            ref Transformation trans,
            Vector3D vector)
        {
            ThrowOut(
                SUTransformationTranslation(
                    ref trans,
                    ref vector),
                "Could not translate transformation.");
        }
    }
}
