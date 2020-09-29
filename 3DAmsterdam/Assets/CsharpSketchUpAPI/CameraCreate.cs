using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUCameraCreate")]
        static extern int SUCameraCreate(
            out IntPtr cameraRef);

        public static void CameraCreate(
            CameraRef cameraRef)
        {
            ThrowOut(
                SUCameraCreate(
                    out cameraRef.intPtr),
                "Could not create camera.");
        }
    }
}
