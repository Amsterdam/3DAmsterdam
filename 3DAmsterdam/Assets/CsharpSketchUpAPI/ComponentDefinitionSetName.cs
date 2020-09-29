using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUComponentDefinitionSetName")]
        static extern int SUComponentDefinitionSetName(
            IntPtr compDefRef,
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string name);

        public static void ComponentDefinitionSetName(
            ComponentDefinitionRef compDefRef,
            string name)
        {
            ThrowOut(
                SUComponentDefinitionSetName(
                    compDefRef.intPtr,
                    name),
                "Could not set definition name.");
        }
    }
}
