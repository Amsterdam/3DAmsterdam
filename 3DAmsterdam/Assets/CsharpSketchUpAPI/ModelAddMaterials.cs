using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUModelAddMaterials")]
        static extern int SUModelAddMaterials(
            IntPtr modelRef,
            long len,
            IntPtr[] materialRefs);

        public static void ModelAddMaterials(
            ModelRef modelRef,
            long len,
            MaterialRef[] materialRefs)
        {
            IntPtr[] intPtrs = new IntPtr[materialRefs.Length];

            for (int i = 0; i < intPtrs.Length; ++i)
            {
                intPtrs[i] = materialRefs[i].intPtr;
            }

            ThrowOut(
                SUModelAddMaterials(
                    modelRef.intPtr,
                    len,
                    intPtrs),
                "Could not add materials.");
        }
    }
}
