using System;
using System.Collections.Generic;

namespace hobd
{

public class OBD2Sensor : Sensor
{
    public Func<OBD2Sensor, double> value;

    public byte[] data_raw;
    
    public OBD2Sensor()
    {
        this.Aliases = new List<string>();
    }
        
    public double GetValue()
    {
        return value(this);
    }
    
    public string ID
    {
        get;
        internal set;
    }
    public string Name
    {
        get;
        internal set;

    }
    public string Description
    {
        get;
        internal set;
    }
    public string Units
    {
        get;
        internal set;
    }
    public string GetDescription(string lang)
    {
        return Description;
    }
    public IEnumerable<string> Aliases
    {
        get;
        internal set;
    }
    
    public int Command
    {
        get;
        internal set;
    }
    
    
    
    public double get(int idx)
    {
        return data_raw[2+idx];
    }

    public double get_bit(int idx, int bit)
    {
        return (data_raw[2+idx] & (1<<bit)) != 0 ? 1 : 0;
    }
    
}

}
