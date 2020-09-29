using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUStringCreate")]
        static extern int SUStringCreate(
            out IntPtr stringRef);

        public static void StringCreate(
            StringRef stringRef)
        {
            ThrowOut(
                SUStringCreate(
                    out stringRef.intPtr),
                "Could not create string.");
        }
    }
}
