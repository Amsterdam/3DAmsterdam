using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUModelAddComponentDefinitions")]
        static extern int SUModelAddComponentDefinitions(
            IntPtr modelRef,
            long len,
            IntPtr[] componentDefRefs);

        public static void ModelAddComponentDefinitions(
            ModelRef modelRef,
            long len,
            ComponentDefinitionRef[] componentDefRefs)
        {
            IntPtr[] intPtrs = new IntPtr[componentDefRefs.Length];

            for (int c = 0; c < intPtrs.Length; ++c)
            {
                intPtrs[c] = componentDefRefs[c].intPtr;
            }

            ThrowOut(
                SUModelAddComponentDefinitions(
                    modelRef.intPtr,
                    len, intPtrs),
                "Could not add definitions.");
        }
    }
}
