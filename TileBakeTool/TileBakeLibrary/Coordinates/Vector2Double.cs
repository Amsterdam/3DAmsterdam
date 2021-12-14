/*
*  Copyright (C) X Gemeente
*              	 X Amsterdam
*				 X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://joinup.ec.europa.eu/software/page/eupl
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using System;
using System.Numerics;

namespace TileBakeLibrary.Coordinates
{
    public struct Vector2Double : IEquatable<Vector3Double>
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

        public bool Equals(Vector2Double other)
        {
            return (other.X == X && other.Y == Y);
        }

        public bool Equals(Vector3Double other)
        {
            return (other.X == X && other.Y == Y);
        }
    }
}
