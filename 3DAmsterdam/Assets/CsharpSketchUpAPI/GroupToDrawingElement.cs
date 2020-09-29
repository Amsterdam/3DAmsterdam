using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUGroupToDrawingElement")]
        static extern IntPtr SUGroupToDrawingElement(
            IntPtr instanceRef);

        public static DrawingElementRef GroupToDrawingElement(
            GroupRef groupRef)
        {
            IntPtr intPtr = SUGroupToDrawingElement(groupRef.intPtr);

            if (intPtr == Invalid)
            {
                ThrowOut(
                    ErrorInvalidOutput,
                    "Could not cast group to drawing element.");

                return null; // Never happens, but compiler wants it.
            }
            else
            {
                DrawingElementRef drawingElementRef = new DrawingElementRef();

                drawingElementRef.intPtr = intPtr;

                return drawingElementRef;
            }
        }
    }
}
