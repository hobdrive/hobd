using System;
using System.Collections.Generic;

namespace hobd
{

/*
 * Accumulator Sensor with two extra properties:
 * 1. It stores its value between runs, until explicit sensor Reset happens
 * 2. It reacts on IAccumulatorSensor events (Reset/Suspend) to accomodate lost of connection.
 */
public class PersistentSensor : CoreSensor, IPersistentSensor, IAccumulatorSensor
{
    
    const int MAX_TIME_INTERVAL = 1*60*1000;

    protected long PrevStamp;

    bool firstRun = true;

    protected virtual bool FirstRun
    {
        get{
            return firstRun || (this.TimeStamp - this.PrevStamp) < 0 || (this.TimeStamp - this.PrevStamp) > MAX_TIME_INTERVAL;
        }
        set{
            firstRun = value;
        }
    }

    public virtual void Reset()
    {
        Value = 0;
        firstRun = true;
    }
    public virtual void Suspend()
    {
        firstRun = true;
    }

    public virtual string StoreState()
    {
        return Value.ToString(UnitsConverter.DefaultNumberFormat) + " " + TimeStamp.ToString();
    }

    public virtual void RestoreState(string raw)
    {
        var raw0 = raw.Split(new char[]{' '});
        var value = raw0[0];
        long ts = DateTimeMs.Now;
        if (raw0.Length > 1)
            ts = long.Parse(raw0[1]);
        // backward compat for possible wrong comma format
        value = value.Replace(",", ".");
        
        this.Value = double.Parse(value, UnitsConverter.DefaultNumberFormat);
        if (Double.IsNaN(this.Value) || Double.IsInfinity(this.Value))
            this.Value = 0;
        this.TimeStamp = ts;

        firstRun = true;
    }

}

}
