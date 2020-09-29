using System.Runtime.InteropServices;

namespace ExLumina.SketchUp.API
{
    public static partial class SU
    {
        [DllImport(LIB, EntryPoint = "SUTerminate")]
        static extern void SUTerminate();

        public static void Terminate()
        {
            SUTerminate();
        }
    }
}
