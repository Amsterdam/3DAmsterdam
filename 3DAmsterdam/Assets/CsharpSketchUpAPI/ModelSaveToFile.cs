using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUModelSaveToFile")]
        static extern int SUModelSaveToFile(
            IntPtr modelRef,
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string name);

        public static void ModelSaveToFile(
            ModelRef modelRef,
            string name)
        {
            ThrowOut(
                SUModelSaveToFile(
                    modelRef.intPtr,
                    name),
                "Could not save model.");
        }
    }
}
