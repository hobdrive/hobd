using System;
using System.Collections.Generic;

namespace hobd
{

/**
 * Calculates total run distance based on speed interpolation
 */
public class DistanceSensor : CoreSensor, IAccumulatorSensor
{
    public static string MeterUnits = "m.";
    DateTime prevStamp;
    bool firstRun = true;
        
    public DistanceSensor()
    {
        ID = "DistanceRun";
        Name = "DistanceRun";
        Description = "Total run distance";
        Units = DistanceSensor.MeterUnits;
    }

    public override void NotifyAddListener(Action<Sensor> listener)
    {
        base.NotifyAddListener(listener);
        if (listenerCount == 1)
            registry.AddListener("Speed", OnSpeedChange);
    }

    public override void NotifyRemoveListener(Action<Sensor> listener)
    {
        base.NotifyRemoveListener(listener);
        if (listenerCount == 0)
            registry.RemoveListener("Speed", OnSpeedChange);
    }

    public void OnSpeedChange(Sensor speed)
    {
        TimeStamp = speed.TimeStamp;
        if (firstRun){
            prevStamp = TimeStamp;
            firstRun = false;
            return;
        }
        Value += speed.Value * 1000 / 3600 * (TimeStamp-prevStamp).TotalMilliseconds / 1000;
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
