using System;
using System.Collections.Generic;

namespace hobd
{

public class IdleTime : PersistentSensor
{
    long prevStamp;
    public int ListenInterval{get; set;}
    public double idle_speed = 5;
                
    public IdleTime()
    {
        ListenInterval = 2000;
    }

    public override void SetRegistry(SensorRegistry registry)
    {
        base.SetRegistry(registry);
        registry.AddListener(OBD2Sensors.Speed, OnSpeedChange, ListenInterval);
        try{
            this.idle_speed = double.Parse(registry.VehicleParameters["idle-speed"], UnitsConverter.DefaultNumberFormat);
        }catch(Exception){}
    }

    public void OnSpeedChange(Sensor speed)
    {
        TimeStamp = speed.TimeStamp;
        if (firstRun) {
            prevStamp = TimeStamp;
            firstRun = false;
            return;
        }
        if (speed.Value > idle_speed){
            firstRun = true;
            return;
        }
        Value += (TimeStamp-prevStamp) / 1000f;
        prevStamp = TimeStamp;
        registry.TriggerListeners(this);
    }

}

}
