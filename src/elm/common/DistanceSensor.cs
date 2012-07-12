using System;
using System.Collections.Generic;

namespace hobd
{

/**
 * Calculates total run distance based on speed interpolation
 */
public class DistanceSensor : PersistentSensor
{
    public int ListenInterval{get; set;}
        
    public DistanceSensor()
    {
        ListenInterval = 5000;
    }

    protected override void Activate()
    {
        registry.AddListener(OBD2Sensors.Speed, OnSpeedChange, ListenInterval);
    }
    protected override void Deactivate()
    {
        registry.RemoveListener(OnSpeedChange);
    }

    public void OnSpeedChange(Sensor speed)
    {
        TimeStamp = speed.TimeStamp;
        if (FirstRun){
            PrevStamp = TimeStamp;
            FirstRun = false;
            return;
        }
        Value += speed.Value * 1000 / 3600 * ((double)(TimeStamp-PrevStamp)) / 1000 / 1000;
        PrevStamp = TimeStamp;
        registry.TriggerListeners(this);
    }

}

}
