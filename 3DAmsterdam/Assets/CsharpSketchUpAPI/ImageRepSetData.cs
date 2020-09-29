using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUImageRepSetData")]
        static extern int SUImageRepSetData(
            IntPtr imageRepRef,
            long width,
            long height,
            long bitsPerPixel,
            long rowPadding,
            byte[] pixelData);

        public static void ImageRepSetData(
            ImageRepRef imageRepRef,
            long width,
            long height,
            long bitsPerPixel,
            long rowPadding,
            byte[] pixelData)
        {
            ThrowOut(
                SUImageRepSetData(
                    imageRepRef.intPtr,
                    width,
                    height,
                    bitsPerPixel,
                    rowPadding,
                    pixelData),
                "Could not set image rep data.");
        }
    }
}
