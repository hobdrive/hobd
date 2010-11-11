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
        registry.AddListener(OBD2Sensors.RPM, OnRPMChange, 10000);
    }

    public void OnRPMChange(Sensor rpm)
    {
        TimeStamp = rpm.TimeStamp;
        if (firstRun) {
            prevStamp = TimeStamp;
            firstRun = false;
            return;
        }
        if (rpm.Value == 0){
            Suspend();
            return;
        }
        Value += ((double)(TimeStamp-prevStamp)) / 1000;
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
