using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct UVQ
        {
            public double u;
            public double v;
            public double q;

            public UVQ(double u, double v, double q)
            {
                this.u = u;
                this.v = v;
                this.q = q;
            }
        }
    }
}
