using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUInitialize")]
        static extern void SUInitialize();

        public static void Initialize()
        {
            SUInitialize();
        }
    }
}
