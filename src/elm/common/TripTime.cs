using System;
using System.Collections.Generic;

namespace hobd
{

public class TripTime : CoreSensor, IAccumulatorSensor
{
    long prevStamp;
    bool firstRun = true;
        
    public TripTime()
    {
    }

    public override void SetRegistry(SensorRegistry registry)
    {
        base.SetRegistry(registry);
        registry.AddListener(OBD2Sensors.Speed, OnChange, 2000);
    }

    public void OnChange(Sensor s)
    {
        TimeStamp = s.TimeStamp;
        if (firstRun) {
            prevStamp = TimeStamp;
            firstRun = false;
            return;
        }
        Value += (TimeStamp-prevStamp) / 1000f;
        prevStamp = TimeStamp;
        registry.TriggerListeners(this);
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

}

}
