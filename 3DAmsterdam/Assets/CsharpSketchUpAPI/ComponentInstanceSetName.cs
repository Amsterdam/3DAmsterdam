using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUComponentInstanceSetName")]
        static extern int SUComponentInstanceSetName(
            IntPtr instanceRef,
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string name);

        public static void ComponentInstanceSetName(
            ComponentInstanceRef instanceRef,
            string name)
        {
            ThrowOut(
                SUComponentInstanceSetName(
                    instanceRef.intPtr,
                    name),
                "Could not set instance name.");
        }
    }
}
