using System;
using System.Collections.Generic;

namespace hobd
{

/**
 * Calculates total run distance based on speed interpolation
 */
public class DistanceSensor : PersistentSensor
{
    long prevStamp;

    public int ListenInterval{get; set;}
        
    public DistanceSensor()
    {
        ListenInterval = 5000;
    }

    public override void Activate()
    {
        registry.AddListener(OBD2Sensors.Speed, OnSpeedChange, ListenInterval);
    }
    public override void Deactivate()
    {
        registry.RemoveListener(OBD2Sensors.Speed, OnSpeedChange);
    }

    public void OnSpeedChange(Sensor speed)
    {
        TimeStamp = speed.TimeStamp;
        if (firstRun){
            prevStamp = TimeStamp;
            firstRun = false;
            return;
        }
        Value += speed.Value * 1000 / 3600 * ((double)(TimeStamp-prevStamp)) / 1000 / 1000;
        prevStamp = TimeStamp;
        registry.TriggerListeners(this);
    }

}

}
