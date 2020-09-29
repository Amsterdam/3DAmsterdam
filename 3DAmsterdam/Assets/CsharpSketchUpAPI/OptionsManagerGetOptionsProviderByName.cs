using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUOptionsManagerGetOptionsProviderByName")]
        static extern int SUOptionsManagerGetOptionsProviderByName(
            IntPtr managerRef,
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string name,
            out IntPtr providerRef);

        public static void OptionsManagerGetOptionsProviderByName(
            OptionsManagerRef managerRef,
            string name,
            OptionsProviderRef providerRef)
        {
            ThrowOut(
                SUOptionsManagerGetOptionsProviderByName(
                    managerRef.intPtr,
                    name, 
                    out providerRef.intPtr),
                "Could not get provider.");
        }
    }
}
