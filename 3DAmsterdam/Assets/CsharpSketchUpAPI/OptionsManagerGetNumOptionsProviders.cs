using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUOptionsManagerGetNumOptionsProviders")]
        static extern int SUOptionsManagerGetNumOptionsProviders(
            IntPtr opMgrRef,
            out long num);

        public static void OptionsManagerGetNumOptionsProviders(
            OptionsManagerRef opMgrRef,
            out long num)
        {
            ThrowOut(
                SUOptionsManagerGetNumOptionsProviders(
                    opMgrRef.intPtr, 
                    out num),
                "Could not get num options providers.");
        }
    }
}
