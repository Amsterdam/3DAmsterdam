using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUOptionsProviderSetValue")]
        static extern int SUOptionsProviderSetValue(
            IntPtr providerRef,
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string naem,
            IntPtr val);

        public static void OptionsProviderSetValue(
            OptionsProviderRef providerRef,
            string name,
            TypedValueRef typedValueRef)
        {
            ThrowOut(
                SUOptionsProviderSetValue(
                    providerRef.intPtr,
                    name,
                    typedValueRef.intPtr),
                "Could not set provider value.");
        }
    }
}
