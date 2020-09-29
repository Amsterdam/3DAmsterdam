using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUCameraSetOrientation")]
        static extern int SUCameraSetOrientation(
            IntPtr cameraRef,
            ref Point3D position,
            ref Point3D target,
            ref Vector3D upVector);

        public static void CameraSetOrientation(
            CameraRef cameraRef,
            Point3D position,
            Point3D target,
            Vector3D upVector)
        {
            ThrowOut(
                SUCameraSetOrientation(
                    cameraRef.intPtr,
                    ref position,
                    ref target,
                    ref upVector),
                "Could not set orientation.");
        }
    }
}
