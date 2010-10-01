using System;
using System.Windows.Forms;
using Fleux.Core;

namespace hobd
{
    internal sealed class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {

//TODO: autoajust from screen size
#if WINCE
            FleuxApplication.TargetDesignDpi = 96;
#else
            // desktop - scale DPI
            FleuxApplication.TargetDesignDpi = 96 * 480 / 800;
#endif
            FleuxApplication.Run(new PanoramaPage1());

        }
        
    }
}
