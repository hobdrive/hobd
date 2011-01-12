using System;
using System.Collections.Generic;

namespace hobd
{

public class OBD2Sensor : CoreSensor
{
    public Func<OBD2Sensor, double> obdValue;
    protected byte[] dataraw;
    
    public OBD2Sensor()
    {
    }

    public int Command { get; set; }
    
    public override double Value { get; protected set; }
    
    public virtual string RawCommand {
        get{
            return "01" + this.Command.ToString("X2");
        }
    }

    public virtual bool SetValue(byte[] dataraw)
    {
        if (dataraw.Length < 2 || dataraw[0] != 0x41 || dataraw[1] != this.Command)
            return false;
        
        this.dataraw = dataraw;
        try{
            this.Value = obdValue(this);
        }catch(Exception e){
            Logger.error("OBD2Sensor", "Fail parsing sensor value", e);
            return false;
        }
        this.TimeStamp = DateTimeMs.Now;
        registry.TriggerListeners(this);
        return true;
    }
    
    public double get(int idx)
    {
        return dataraw[2+idx];
    }

    public double get_bit(int idx, int bit)
    {
        return (dataraw[2+idx] & (1<<bit)) != 0 ? 1 : 0;
    }
    
}

}
