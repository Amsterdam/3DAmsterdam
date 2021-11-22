using System;

namespace TileBakeLibrary.Coordinates
{
    public struct Vector2Double
    {
        public double X;
        public double Y;

        public Vector2Double(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public override string ToString()
        {
            return $"x:{X} y:{Y}";
        }
    }
}
