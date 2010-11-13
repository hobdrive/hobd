using System;

namespace hobd{

class HOBRunner
{

    [STAThread]
    private static void Main(string[] args)
    {
        HOBD.Init();
        HOBD.Run(new HomePage());
    }

}

}