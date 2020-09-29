using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUModelGetComponentDefinitions")]
        static extern int SUModelGetComponentDefinitions(
            IntPtr modelRef,
            long numRequested,
            IntPtr[] componentDefinitionRefs,
            out long numReturned);

        public static void ModelGetComponentDefinitions(
            ModelRef modelRef,
            long numRequested,
            ComponentDefinitionRef[] componentDefinitionRefs,
            out long numReturned)
        {
            IntPtr[] intPtrs = new IntPtr[numRequested];

            ThrowOut(
                SUModelGetComponentDefinitions(
                    modelRef.intPtr,
                    numRequested,
                    intPtrs,
                    out numReturned),
                "Could not get component definitions.");

            for (int i = 0; i < numReturned; ++i)
            {
                componentDefinitionRefs[i] = new ComponentDefinitionRef();
                componentDefinitionRefs[i].intPtr = intPtrs[i];
            }
        }
    }
}
