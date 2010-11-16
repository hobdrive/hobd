using System;

namespace hobd{

class HOBRunner
{

    [STAThread]
    private static void Main(string[] args)
    {
        if (!HOBD.Init())
            return;
        HOBD.Run("hobd.HomePage");
    }

}

}