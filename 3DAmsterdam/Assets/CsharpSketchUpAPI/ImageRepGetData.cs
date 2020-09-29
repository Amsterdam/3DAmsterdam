using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUImageRepGetData")]
        static extern int SUImageRepGetData(
            IntPtr imageRepRef,
            long dataSize,
            byte[] pixelData);

        public static void ImageRepGetData(
            ImageRepRef imageRepRef,
            long dataSize,
            byte[] pixelData)
        {
            ThrowOut(
                SUImageRepGetData(
                    imageRepRef.intPtr,
                    dataSize,
                    pixelData),
                "Could not get image rep data.");
        }
    }
}
