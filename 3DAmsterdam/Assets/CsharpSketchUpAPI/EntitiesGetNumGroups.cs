using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUEntitiesGetNumGroups")]
        static extern int SUEntitiesGetNumGroups(
            IntPtr entititesRef,
            out long numGroups);

        public static void EntitiesGetNumGroups(
            EntitiesRef entitiesRef,
            out long numGroups)
        {
            ThrowOut(
                SUEntitiesGetNumGroups(
                    entitiesRef.intPtr,
                    out numGroups),
                "Could not get number of groups.");
        }
    }
}
