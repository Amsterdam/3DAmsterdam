using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUComponentDefinitionGetDescription")]
        static extern int SUComponentDefinitionGetDescription(
            IntPtr componentDefinitionRef,
            out IntPtr stringRef);

        public static void ComponentDefinitionGetDescription(
            ComponentDefinitionRef componentDefinitionRef,
            StringRef stringRef)
        {
            ThrowOut(
                SUComponentDefinitionGetDescription(
                    componentDefinitionRef.intPtr,
                    out stringRef.intPtr),
                "Could not get component definition description.");
        }
    }
}
