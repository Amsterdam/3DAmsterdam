using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUTypedValueSetInt32")]
        static extern int SUTypedValueSetInt32(
            IntPtr typedValueRef,
            int val);

        public static void TypedValueSetInt32(
            TypedValueRef typedValueRef,
            int val)
        {
            ThrowOut(
                SUTypedValueSetInt32(
                    typedValueRef.intPtr,
                    val),
                "Could not set typed value.");
        }
    }
}
