using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        // This passes a reference to a Point3D, which could cause problems if the reference
        // were stored (the GC could move it or collect it). But, testing confirms that the
        // values are actually copied. That is, changing the members of the Point3D struct
        // after this returns does not affect the model, even if the changes are made before
        // calling SUEntitiesFill.

        [DllImport(LIB, EntryPoint = "SUGeometryInputAddVertex")]
        static extern int SUGeometryInputAddVertex(
            IntPtr geometryInputRef,
            ref Point3D vertex);

        public static void GeometryInputAddVertex(
            GeometryInputRef geometryInputRef,
            Point3D vertex)
        {
            ThrowOut(
                SUGeometryInputAddVertex(
                    geometryInputRef.intPtr,
                    ref vertex),
                "Could not add vertex.");
        }
    }
}
