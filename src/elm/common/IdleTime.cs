using System;
using System.Collections.Generic;

namespace hobd
{

public class IdleTime : PersistentSensor
{
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
        if (FirstRun) {
            PrevStamp = TimeStamp;
            FirstRun = false;
            return;
        }
        if (speed.Value > IdleSpeed){
            FirstRun = true;
            return;
        }
        Value += (TimeStamp-PrevStamp) / 1000f;
        PrevStamp = TimeStamp;
        registry.TriggerListeners(this);
    }

}

}
