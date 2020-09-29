using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUModelGetMaterials")]
        static extern int SUModelGetMaterials(
            IntPtr modelRef,
            long numRequested,
            IntPtr[] materialRefs,
            out long numReturned);

        public static void ModelGetMaterials(
            ModelRef modelRef,
            long numRequested,
            MaterialRef[] materialRefs,
            out long numReturned)
        {
            IntPtr[] intPtrs = new IntPtr[numRequested];

            ThrowOut(
                SUModelGetMaterials(
                    modelRef.intPtr,
                    numRequested,
                    intPtrs,
                    out numReturned),
                "Could not get materials.");

            for (int i = 0; i < numReturned; ++i)
            {
                materialRefs[i] = new MaterialRef();
                materialRefs[i].intPtr = intPtrs[i];
            }
        }
    }
}
