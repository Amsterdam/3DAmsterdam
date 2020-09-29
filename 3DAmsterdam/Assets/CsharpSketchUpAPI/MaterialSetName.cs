using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUMaterialSetName")]
        static extern int SUMaterialSetName(
            IntPtr materialRef,
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string name);

        public static void MaterialSetName(
            MaterialRef materialRef,
            string name)
        {
            ThrowOut(
                SUMaterialSetName(
                    materialRef.intPtr,
                    name),
                "Could not set name.");
        }
    }
}
