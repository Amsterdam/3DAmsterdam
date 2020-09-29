using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct Transformation
        {
            public double m00;
            public double m10;
            public double m20;
            public double m30;
            public double m01;
            public double m11;
            public double m21;
            public double m31;
            public double m02;
            public double m12;
            public double m22;
            public double m32;
            public double m03;
            public double m13;
            public double m23;
            public double m33;

            /// <summary>
            /// [,] operator, indexes by row, column.
            /// </summary>
            /// <param name="r"></param>
            /// <param name="c"></param>
            /// <returns></returns>
            unsafe public double this[int r, int c]
            {
                get
                {
                    fixed (double* m = &m00)
                    {
                        return m[c * 4 + r];
                    }
                }
            }
        }
    }
}
