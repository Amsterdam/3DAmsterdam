using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUEntitiesFill")]
        static extern int SUEntitiesFill(
            IntPtr entitiesRef,
            IntPtr geometryRef,
            bool weld);

        public static void EntitiesFill(
            EntitiesRef entitiesRef,
            GeometryInputRef geometryInputRef,
            bool weld)
        {
            ThrowOut(
                SUEntitiesFill(
                    entitiesRef.intPtr,
                    geometryInputRef.intPtr,
                    weld),
                "Could not fill entities.");
        }
    }
}
