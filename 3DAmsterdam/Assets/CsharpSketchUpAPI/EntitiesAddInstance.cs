using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUEntitiesAddInstance")]
        static extern int SUEntitiesAddInstance(
            IntPtr entitiesRef,
            IntPtr instanceRef,
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string name);

        public static void EntitiesAddInstance(
            EntitiesRef entitiesRef,
            ComponentInstanceRef instanceRef,
            string name)
        {
            ThrowOut(
                SUEntitiesAddInstance(
                    entitiesRef.intPtr,
                    instanceRef.intPtr,
                    name),
                "Could not add instance.");
        }
    }
}
