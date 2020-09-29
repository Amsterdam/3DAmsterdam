using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUModelGetOptionsManager")]
        static extern int SUModelGetOptionsManager(
            IntPtr modelRef,
            out IntPtr managerRef);

        public static void ModelGetOptionsManager(
            ModelRef modelRef,
            OptionsManagerRef managerRef)
        {
            ThrowOut(
                SUModelGetOptionsManager(
                    modelRef.intPtr,
                    out managerRef.intPtr),
                "Could not get options manager.");
        }
    }
}
