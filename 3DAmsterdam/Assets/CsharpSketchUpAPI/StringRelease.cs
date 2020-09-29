using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUStringRelease")]
        static extern int SUStringRelease(
            ref IntPtr stringRef);

        public static void StringRelease(
            StringRef stringRef)
        {
            ThrowOut(
                SUStringRelease(
                    ref stringRef.intPtr),
                "Could not release string.");
        }
    }
}
