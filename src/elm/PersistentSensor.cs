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
    protected bool firstRun = true;
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
        return Value.ToString(UnitsConverter.DefaultNumberFormat);
    }

    public virtual void RestoreState(string raw)
    {
        // backward compat for possible wrong comma format
        raw = raw.Replace(",", ".");
        Value = double.Parse(raw, UnitsConverter.DefaultNumberFormat);
        // TODO: Should TS also be stored/restored?
        TimeStamp = DateTimeMs.Now;
        firstRun = true;
    }

}

}
