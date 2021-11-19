using System;

namespace TileBakeLibrary.Coordinates
{
    public struct Vector2Double
    {
        public double x;
        public double y;

        public Vector2Double(double X, double Y)
        {
            x = X;
            y = Y;
        }
        public override string ToString()
        {
            return $"x:{x} y:{y}";
        }
    }
}
