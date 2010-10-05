using System;

namespace hobd
{

public class SimpleSensor : Sensor
{
    string name;
    int command;
    public Func<SimpleSensor, double> value;
    
    public SimpleSensor(string name, int command)
    {
        this.name = name;
        this.command = command;
    }
        
    public double getValue()
    {
        return value(this);
    }
    
    public double get(int idx)
    {
        return 0;
    }

    public double get_bit(int idx, int bit)
    {
        return 0;
    }
    
}

}
