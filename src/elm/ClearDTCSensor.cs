using System;
using System.Collections.Generic;

namespace hobd
{

/**
 * virtual sensor for clearing DTC codes.
 */
public class ClearDTCSensor : OBD2Sensor
{
    public const int ReadInterval = 500 * 60 * 1000;

    public ClearDTCSensor()
    {
        RawCommand = "04";
    }

    public override bool SetValue(byte[] dataraw)
    {
        this.TimeStamp = DateTimeMs.Now;
        registry.TriggerListeners(this);
        return true;
    }

}

}
