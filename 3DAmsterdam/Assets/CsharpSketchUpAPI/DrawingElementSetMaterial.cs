using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUDrawingElementSetMaterial")]
        static extern int SUDrawingElementSetMaterial(
            IntPtr drawingElementRef,
            IntPtr materialRef);

        public static void DrawingElementSetMaterial(
            DrawingElementRef drawingElementRef,
            MaterialRef materialRef)
        {
            ThrowOut(
                SUDrawingElementSetMaterial(
                    drawingElementRef.intPtr,
                    materialRef.intPtr),
                    "Could not set drawing element material");
        }
    }
}
