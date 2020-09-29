using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUEntitiesAddGroup")]
        static extern int SUEntitiesAddGroup(
            IntPtr entitiesRef,
            IntPtr groupRef);

        public static void EntitiesAddGroup(
            EntitiesRef entitiesRef,
            GroupRef groupRef)
        {
            ThrowOut(
                SUEntitiesAddGroup(
                    entitiesRef.intPtr,
                    groupRef.intPtr),
                    "Could not add group.");
        }
    }
}
