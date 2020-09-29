using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUComponentDefinitionCreateInstance")]
        static extern int SUComponentDefinitionCreateInstance(
            IntPtr compDefRef,
            out IntPtr instanceRef);

        public static void ComponentDefinitionCreateInstance(
            ComponentDefinitionRef compDefRef,
            ComponentInstanceRef instanceRef)
        {
            ThrowOut(
                SUComponentDefinitionCreateInstance(
                    compDefRef.intPtr,
                    out instanceRef.intPtr),
                "Could not create instance.");
        }
    }
}
