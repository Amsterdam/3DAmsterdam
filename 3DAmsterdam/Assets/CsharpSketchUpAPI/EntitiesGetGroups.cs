using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUEntitiesGetGroups")]
        static extern int SUEntitiesGetGroups(
            IntPtr entitiesRef,
            long len,
            IntPtr[] groupRefs,
            out long count);

        public static void EntitiesGetGroups(
            EntitiesRef entitiesRef,
            long len,
            GroupRef[] groupRefs,
            out long count)
        {
            IntPtr[] intPtrs = new IntPtr[groupRefs.Length];

            ThrowOut(
                SUEntitiesGetGroups(
                    entitiesRef.intPtr,
                    len,
                    intPtrs,
                    out count),
                "Could not get entities groups.");

            for (int i = 0; i < groupRefs.Length; ++i)
            {
                groupRefs[i] = new GroupRef();
                groupRefs[i].intPtr = intPtrs[i];
            }
        }
    }
}
