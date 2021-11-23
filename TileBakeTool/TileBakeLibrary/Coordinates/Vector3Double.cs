using System;
using System.Numerics;

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

		public static explicit operator Vector3Double(Vector3 v)
		{
            return new Vector3Double(v.X, v.Y, v.Z);
		}

		public static explicit operator Vector3(Vector3Double v)
		{
            return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
        }
	}
}
