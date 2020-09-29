using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUEntitiesGetInstances")]
        static extern int SUEntitiesGetInstances(
            IntPtr entitiesRef,
            long len,
            IntPtr[] instanceRefs,
            out long count);

        public static void EntitiesGetInstances(
            EntitiesRef entitiesRef,
            long len,
            ComponentInstanceRef[] instanceRefs,
            out long count)
        {
            IntPtr[] intPtrs = new IntPtr[instanceRefs.Length];

            ThrowOut(
                SUEntitiesGetInstances(
                    entitiesRef.intPtr,
                    len,
                    intPtrs,
                    out count),
                "Could not get entities instances.");

            for (int i = 0; i < instanceRefs.Length; ++i)
            {
                instanceRefs[i] = new ComponentInstanceRef();
                instanceRefs[i].intPtr = intPtrs[i];
            }
        }
    }
}
