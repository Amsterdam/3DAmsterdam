using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUComponentDefinitionGetName")]
        static extern int SUComponentDefinitionGetName(
            IntPtr componentDefinitionRef,
            out IntPtr stringRef);

        public static void ComponentDefinitionGetName(
            ComponentDefinitionRef componentDefinitionRef,
            StringRef stringRef)
        {
            ThrowOut(
                SUComponentDefinitionGetName(
                    componentDefinitionRef.intPtr,
                    out stringRef.intPtr),
                "Could not get component definition name.");
        }
    }
}
