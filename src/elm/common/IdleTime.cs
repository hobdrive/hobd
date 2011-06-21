using System;
using System.Collections.Generic;

namespace hobd
{

public class IdleTime : PersistentSensor
{
    long prevStamp;
    public int ListenInterval{get; set;}
    public double IdleSpeed{get; set;}
                
    public IdleTime()
    {
        ListenInterval = 2000;
        IdleSpeed = 5;
    }

    public override void SetRegistry(SensorRegistry registry)
    {
        base.SetRegistry(registry);
        try{
            IdleSpeed = double.Parse(registry.VehicleParameters["idle-speed"], UnitsConverter.DefaultNumberFormat);
        }catch(Exception){}
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
        if (firstRun) {
            prevStamp = TimeStamp;
            firstRun = false;
            return;
        }
        if (speed.Value > IdleSpeed){
            firstRun = true;
            return;
        }
        Value += (TimeStamp-prevStamp) / 1000f;
        prevStamp = TimeStamp;
        registry.TriggerListeners(this);
    }

}

}
