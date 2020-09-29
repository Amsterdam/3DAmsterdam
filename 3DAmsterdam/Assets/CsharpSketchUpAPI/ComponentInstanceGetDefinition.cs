using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUComponentInstanceGetDefinition")]
        static extern int SUComponentInstanceGetDefinition(
            IntPtr instanceRef,
            out IntPtr componentDefinitionRef);

        public static void ComponentInstanceGetDefinition(
            ComponentInstanceRef instanceRef,
            ComponentDefinitionRef componentDefinitionRef)
        {
            ThrowOut(
                SUComponentInstanceGetDefinition(
                    instanceRef.intPtr,
                    out componentDefinitionRef.intPtr),
                "Could not get component definition.");
        }
    }
}
