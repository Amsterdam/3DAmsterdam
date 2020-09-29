using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUEntitiesAddFaces")]
        static extern int SUEntitiesAddFaces(
            IntPtr entitiesRef,
            int len,
            IntPtr[] faces);

        public static void EntitiesAddFaces(
            EntitiesRef entitiesRef,
            int len,
            FaceRef[] faces)
        {
            IntPtr[] intPtrs = new IntPtr[faces.Length];

            for (int face = 0; face < faces.Length; ++face)
            {
                intPtrs[face] = faces[face].intPtr;
            }

            ThrowOut(
                SUEntitiesAddFaces(
                    entitiesRef.intPtr, 
                    len,
                    intPtrs),
                "Could not add faces.");
        }
    }
}
