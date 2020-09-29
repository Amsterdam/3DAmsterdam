using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUStringGetUTF8Length")]
        static extern int SUStringGetUTF8Length(
            IntPtr name,
            out long nameLength);

        public static void StringGetUTF8Length(
            StringRef stringRef,
            out long nameLength)
        {
            ThrowOut(
                SUStringGetUTF8Length(
                    stringRef.intPtr,
                    out nameLength),
                "Could not get UTF8 length.");
        }
    }
}
