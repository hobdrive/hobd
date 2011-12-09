using System;
using System.Collections.Generic;

namespace hobd
{

public class TripTime : PersistentSensor
{
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
        if (FirstRun) {
            PrevStamp = TimeStamp;
            FirstRun = false;
            return;
        }
        Value += (TimeStamp-PrevStamp) / 1000f;
        PrevStamp = TimeStamp;
        registry.TriggerListeners(this);
    }

}

}
