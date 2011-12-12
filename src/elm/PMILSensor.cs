using System;
using System.Collections.Generic;

namespace hobd
{

/**
 * PMIL sensor reads all pending DTCs and works in the same way as MILSensor does
 */
public class PMILSensor : MILSensor
{

    public PMILSensor()
    {
        RawCommand = "07";
    }
}

}
