using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUStringGetUTF8")]
        static extern int SUStringGetUTF8(
            IntPtr stringRef,
            long req,
            byte[] buff,
            out long ret);

        public static void StringGetUTF8(
            StringRef stringRef,
            long req,
            byte[] buff,
            out long ret)
        {
            ThrowOut(
                SUStringGetUTF8(
                    stringRef.intPtr,
                    req,
                    buff,
                    out ret),
                "Could not get UTF8 string");
        }
    }
}
