using System;

namespace TileBakeLibrary.Coordinates
{
    public struct Vector3Double
    {
        public double x;
        public double y;
        public double z;
        public Vector3Double(double X, double Y, double Z)
        {
            x = X;
            y = Y;
            z = Z;
        }
        public override string ToString()
        {
            return $"x:{x} y:{y} z:{z}";
        }
    }
}
