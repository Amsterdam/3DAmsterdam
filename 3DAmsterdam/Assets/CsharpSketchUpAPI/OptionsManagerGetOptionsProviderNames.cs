using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUOptionsManagerGetOptionsProviderNames")]
        static extern int SUOptionsManagerGetOptionsProviderNames(
            IntPtr opMgrRef,
            long len,
            IntPtr[] stringRef,
            out long retrieved);

        public static void OptionsManagerGetOptionsProviderNames(
            OptionsManagerRef opMgrRef,
            long len,
            StringRef[] strings,
            out long retrieved)
        {
            IntPtr[] intPtrs = new IntPtr[strings.Length];

            ThrowOut(
                SUOptionsManagerGetOptionsProviderNames(
                    opMgrRef.intPtr,
                    len,
                    intPtrs,
                    out retrieved),
                "Could not get provider names.");

            for (int i = 0; i < strings.Length; ++i)
            {
                strings[i].intPtr = intPtrs[i];
            }
        }
    }
}
