using System;
using System.Numerics;

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

        public static explicit operator Vector2Double(Vector2 v)
        {
            return new Vector2Double(v.X, v.Y);
        }

        public static explicit operator Vector2(Vector2Double v)
        {
            return new Vector2((float)v.X, (float)v.Y);
        }
    }
}
