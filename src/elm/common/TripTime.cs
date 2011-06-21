using System;
using System.Collections.Generic;

namespace hobd
{

public class TripTime : PersistentSensor
{
    long prevStamp;

    public int ListenInterval{get; set;}
        
    public TripTime()
    {
        ListenInterval = 2000;
    }

    public override void Activate()
    {
        registry.AddListener(OBD2Sensors.Speed, OnChange, ListenInterval);
    }
    public override void Deactivate()
    {
        registry.RemoveListener(OBD2Sensors.Speed, OnChange);
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

}

}
