using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUOptionsProviderGetNumKeys")]
        static extern int SUOptionsProviderGetNumKeys(
            IntPtr opProvRef,
            out long num);

        public static void OptionsProviderGetNumKeys(
            OptionsProviderRef opProvRef,
            out long num)
        {
            ThrowOut(
                SUOptionsProviderGetNumKeys(
                    opProvRef.intPtr,
                    out num),
                "Could not get num keys.");
        }
    }
}
