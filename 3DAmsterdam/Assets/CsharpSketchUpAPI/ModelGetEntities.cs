using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUModelGetEntities")]
        static extern int SUModelGetEntities(
            IntPtr modelRef,
            out IntPtr entitiesRef);

        public static void ModelGetEntities(
            ModelRef modelRef,
            EntitiesRef entitiesRef)
        {
            ThrowOut(
                SUModelGetEntities(
                    modelRef.intPtr,
                    out entitiesRef.intPtr),
                "Could not get model entities.");
        }
    }
}
