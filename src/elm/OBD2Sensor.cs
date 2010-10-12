using System;
using System.Collections.Generic;

namespace hobd
{

public class OBD2Sensor : Sensor
{
    string id;
    string name;
    string descr;
    int command;
    
    IEnumerable<string> aliases;
    public Func<OBD2Sensor, double> value;

    public byte[] data_raw;
    
    public OBD2Sensor(string id, string name, string descr, int command)
    {
        this.id = id;
        this.name = name;
        this.descr = descr;
        this.command = command;
        this.aliases = new List<string>();
    }
        
    public double GetValue()
    {
        return value(this);
    }
    
    public string ID
    {
        get{
            return id;
        }
    }
    public string Name
    {
        get{
            return name;
        }
    }
    public string Description
    {
        get{
            return descr;
        }
    }
    public string GetDescription(string lang)
    {
        return Description;
    }
    public IEnumerable<string> Aliases
    {
        get{
            return aliases;
        }
    }
    
    public int Command
    {
        get{
            return command;
        }
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
