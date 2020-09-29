using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUGroupCreate")]
        static extern int SUGroupCreate(
            out IntPtr groupRef);

        public static void GroupCreate(
            GroupRef groupRef)
        {
            ThrowOut(
                SUGroupCreate(
                    out groupRef.intPtr),
                "Could not create group.");
        }
    }
}
