using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUDrawingElementGetMaterial")]
        static extern int SUDrawingElementGetMaterial(
            IntPtr drawingElementRef,
            out IntPtr materialRef);

        public static void DrawingElementGetMaterial(
            DrawingElementRef drawingElementRef,
            MaterialRef materialRef)
        {
            ThrowOut(
                SUDrawingElementGetMaterial(
                    drawingElementRef.intPtr,
                    out materialRef.intPtr),
                    "Could not get drawing element material");
        }
    }

}
