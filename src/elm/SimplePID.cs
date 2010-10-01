using System;

namespace hobd
{

public class SimplePID : PID
{
    string name;
    int command;
    
    public SimplePID(string name, int command)
    {
        this.name = name;
        this.command = command;
    }
    
    public Func<SimplePID, double> value;
    
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
