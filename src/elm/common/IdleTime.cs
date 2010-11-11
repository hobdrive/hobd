using System;
using System.Collections.Generic;

namespace hobd
{

public class IdleTime : CoreSensor, IAccumulatorSensor
{
    long prevStamp;
    bool firstRun = true;
        
    public IdleTime()
    {
    }

    public override void SetRegistry(SensorRegistry registry)
    {
        base.SetRegistry(registry);
        registry.AddListener(OBD2Sensors.Speed, OnSpeedChange, 5000);
    }

    public void OnSpeedChange(Sensor speed)
    {
        TimeStamp = speed.TimeStamp;
        if (firstRun) {
            prevStamp = TimeStamp;
            firstRun = false;
            return;
        }
        if (speed.Value > 5){
            firstRun = true;
            return;
        }
        Value += (TimeStamp-prevStamp) / 1000;
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
