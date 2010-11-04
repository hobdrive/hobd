using System;
using System.Collections.Generic;

namespace hobd
{

/**
 * Calculates total run distance based on speed interpolation
 */
public class DistanceSensor : CoreSensor, IAccumulatorSensor
{
    long prevStamp;
    bool firstRun = true;

    public int ListenInterval{get; set;}
        
    public DistanceSensor()
    {
        ListenInterval = 5000;
    }

    public override void SetRegistry(SensorRegistry registry)
    {
        base.SetRegistry(registry);
        registry.AddListener(OBD2Sensors.Speed, OnSpeedChange, ListenInterval);
    }

    public void OnSpeedChange(Sensor speed)
    {
        TimeStamp = speed.TimeStamp;
        if (firstRun){
            prevStamp = TimeStamp;
            firstRun = false;
            return;
        }
        Value += speed.Value * 1000 / 3600 * (TimeStamp-prevStamp) / 1000;
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
