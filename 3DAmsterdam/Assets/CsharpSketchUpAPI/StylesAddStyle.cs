using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUStylesAddStyle")]
        static extern int SUStylesAddStyle(
            IntPtr stylesRef,
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string path,
            bool activate);

        public static void StylesAddStyle(
            StylesRef stylesRef,
            string path,
            bool activate)
        {
            ThrowOut(
                SUStylesAddStyle(
                    stylesRef.intPtr,
                    path, activate),
                "Could not add style.");
        }
    }
}
