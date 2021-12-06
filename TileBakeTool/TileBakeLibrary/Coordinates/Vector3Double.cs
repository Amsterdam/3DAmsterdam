using System;
using System.Numerics;

namespace TileBakeLibrary.Coordinates
{
    public struct Vector3Double : IEquatable<Vector3Double>
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

        public static double Distance(Vector3Double left, Vector3Double right){
            return Magnitude(left - right);
        }

        public static double Magnitude(Vector3Double left)
        {
            return Math.Sqrt(left.X * left.X + left.Y*left.Y + left.Z*left.Z);
        }

        public bool Equals(Vector3Double other)
		{
			return (other.X == X && other.Y == Y && other.Z == Z);
		}
        public static bool operator == (Vector3Double left, Vector3Double right)
        {
            if (left.X==right.X && left.Y==right.Y&&left.Z==right.Z)
            {
                return true;
            }
            return false;

        }
        public static bool operator !=(Vector3Double left, Vector3Double right)
        {
            if (left==right)
            {
                return false;
            }
            return true;
        }

        public static Vector3Double operator -(Vector3Double left, Vector3Double right)
        {
            return new Vector3Double(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }
        public static Vector3Double operator +(Vector3Double left, Vector3Double right)
        {
            return new Vector3Double(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
    }
}
