using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUGetColorOrder")]
        static extern SU.ColorOrder SUGetColorOrder();

        public static SU.ColorOrder GetColorOrder()
        {
            return SUGetColorOrder();
        }
    }
}
