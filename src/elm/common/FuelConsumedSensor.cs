using System;
using System.Collections.Generic;

namespace hobd
{

/**
 * Calculates total fuel consumed during the trip
 * Uses the underlying Liters Per Hour sensor
 */
public class FuelConsumedSensor : PersistentSensor
{
    long prevStamp;

    public int ListenInterval{get; set;}
        
    public FuelConsumedSensor()
    {
        ListenInterval = 2000;
    }

    public override void SetRegistry(SensorRegistry registry)
    {
        base.SetRegistry(registry);
        registry.AddListener("LitersPerHour", OnChange, ListenInterval);
    }

    void OnChange(Sensor lph)
    {
        TimeStamp = lph.TimeStamp;
        if (firstRun){
            prevStamp = TimeStamp;
            firstRun = false;
            return;
        }
        Value += lph.Value / 3600 * ((double)(TimeStamp-prevStamp)) / 1000;
        prevStamp = TimeStamp;
        registry.TriggerListeners(this);
    }

    public void SetValue(double value)
    {
        this.Value = value;
    }

}

}
