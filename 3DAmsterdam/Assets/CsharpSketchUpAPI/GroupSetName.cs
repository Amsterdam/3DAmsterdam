using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUGroupSetName")]
        static extern int SUGroupSetName(
            IntPtr groupRef,
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string name);

        public static void GroupSetName(
            GroupRef groupRef,
            string name)
        {
            ThrowOut(
                SUGroupSetName(
                    groupRef.intPtr,
                    name),
                "Could not set group name.");
        }
    }
}
