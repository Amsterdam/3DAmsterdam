using System;
using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUComponentInstanceToDrawingElement")]
        static extern IntPtr SUComponentInstanceToDrawingElement(
            IntPtr instanceRef);

        public static DrawingElementRef ComponentInstanceToDrawingElement(
            ComponentInstanceRef instanceRef)
        {
            IntPtr intPtr = SUComponentInstanceToDrawingElement(instanceRef.intPtr);

            if (intPtr == Invalid)
            {
                ThrowOut(
                    ErrorInvalidOutput,
                    "Could not cast instance to drawing element.");

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
