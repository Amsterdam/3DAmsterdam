using System;

namespace TileBakeLibrary.Coordinates
{
    public struct Vector3Double
    {
        public double X;
        public double Y;
        public double Z;
        public Vector3Double(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public override string ToString()
        {
            return $"x:{X} y:{Y} z:{Z}";
        }
    }
}
