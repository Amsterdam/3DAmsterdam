using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUEntitiesGetNumFaces")]
        static extern int SUEntitiesGetNumFaces(
            IntPtr entititesRef,
            out long numFaces);

        public static void EntitiesGetNumFaces(
            EntitiesRef entitiesRef,
            out long numFaces)
        {
            ThrowOut(
                SUEntitiesGetNumFaces(
                    entitiesRef.intPtr,
                    out numFaces),
                "Could not get number of faces.");
        }
    }
}
