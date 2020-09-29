using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUEntitiesGetNumInstances")]
        static extern int SUEntitiesGetNumInstances(
            IntPtr entititesRef,
            out long numInstances);

        public static void EntitiesGetNumInstances(
            EntitiesRef entitiesRef,
            out long numInstances)
        {
            ThrowOut(
                SUEntitiesGetNumInstances(
                    entitiesRef.intPtr,
                    out numInstances),
                "Could not get number of instances.");
        }
    }
}
