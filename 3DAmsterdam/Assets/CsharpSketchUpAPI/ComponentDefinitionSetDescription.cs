using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUComponentDefinitionSetDescription")]
        static extern int SUComponentDefinitionSetDescription(
            IntPtr compDefRef,
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string description);

        public static void ComponentDefinitionSetDescription(
            ComponentDefinitionRef compDefRef,
            string description)
        {
            ThrowOut(
                SUComponentDefinitionSetDescription(
                    compDefRef.intPtr,
                    description),
                "Could not set definition description.");
        }
    }
}
