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

    public bool Active{ get; set; }

    public ClearDTCSensor()
    {
        Active = false;
    }

    public override string RawCommand {
        get{
            if (Active)
                return "04";
            return null;
        }
    }

    public override bool SetValue(byte[] dataraw)
    {
        Active = false;
        this.TimeStamp = DateTimeMs.Now;
        registry.TriggerListeners(this);
        return true;
    }

}

}
