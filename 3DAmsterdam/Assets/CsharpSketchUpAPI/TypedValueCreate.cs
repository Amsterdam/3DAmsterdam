using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUTypedValueCreate")]
        static extern int SUTypedValueCreate(
            out IntPtr typedValueRef);

        public static void TypedValueCreate(
            TypedValueRef typedValueRef)
        {
            ThrowOut(
                SUTypedValueCreate(
                    out typedValueRef.intPtr),
                "Could not create typed value.");
        }
    }
}
