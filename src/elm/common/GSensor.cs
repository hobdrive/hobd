using System;
using System.Collections.Generic;

namespace hobd
{

/**
 * Calculates total run distance based on speed interpolation
 */
public class GSensor : CoreSensor
{
    long prevStamp;
    double prevValue;

    protected bool firstRun = true;

    public int ListenInterval{get; set;}
        
    public GSensor()
    {
        ListenInterval = 0;
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
            prevValue = speed.Value;
            firstRun = false;
            return;
        }
        Value = (speed.Value - prevValue) * 1000 / 3600  /  ((double)(TimeStamp-prevStamp)) * 1000;
        prevStamp = TimeStamp;
        prevValue = speed.Value;
        registry.TriggerListeners(this);
    }

}

}
