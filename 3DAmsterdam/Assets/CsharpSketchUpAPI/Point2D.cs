using System.Runtime.InteropServices;
    
namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct Point2D
        {
            public double x;
            public double y;

            public Point2D(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
        }
    }
}
