using System;
using System.Collections.Generic;

namespace hobd
{

public class OBD2Sensor : CoreSensor
{
    internal Func<OBD2Sensor, double> obdValue;
    internal byte[] dataraw;
    
    public OBD2Sensor()
    {
    }

    public int Command { get; internal set; }
    
    public override double Value { get; protected set; }
    
    internal void SetValue(byte[] dataraw)
    {
        this.dataraw = dataraw;
        this.Value = obdValue(this);
        this.TimeStamp = DateTime.Now;
        registry.TriggerListeners(this);
    }
    
    internal double get(int idx)
    {
        return dataraw[2+idx];
    }

    internal double get_bit(int idx, int bit)
    {
        return (dataraw[2+idx] & (1<<bit)) != 0 ? 1 : 0;
    }
    
}

}
