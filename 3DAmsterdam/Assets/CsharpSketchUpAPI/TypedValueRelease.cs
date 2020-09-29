using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUTypedValueRelease")]
        static extern int SUTypedValueRelease(
            ref IntPtr typedValueRef);

        public static void TypedValueRelease(
            TypedValueRef typedValueRef)
        {
            ThrowOut(
                SUTypedValueRelease(
                    ref typedValueRef.intPtr),
                "Could not release typed value.");
        }
    }
}
