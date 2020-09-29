using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUTransformationMultiply")]
        static extern int SUTransformationMultiply(
            ref Transformation trans1,
            ref Transformation trans2,
            ref Transformation outTrans);

        public static void TransformationMultiply(
            ref Transformation trans1,
            ref Transformation trans2,
            ref Transformation outTrans)
        {
            ThrowOut(
                SUTransformationMultiply(
                    ref trans1,
                    ref trans2,
                    ref outTrans),
                "Could not multiply transformations.");
        }
    }
}
