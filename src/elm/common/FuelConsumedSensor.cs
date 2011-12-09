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
    public int ListenInterval{get; set;}
        
    public FuelConsumedSensor()
    {
        ListenInterval = 2000;
    }

    public override void Activate()
    {
        Logger.trace("FCS", "lph: "+registry.Sensor(OBD2Sensors.LitersPerHour).ID);
        registry.AddListener(OBD2Sensors.LitersPerHour, OnChange, ListenInterval);
    }
    public override void Deactivate()
    {
        registry.RemoveListener(OBD2Sensors.LitersPerHour, OnChange);
    }

    void OnChange(Sensor lph)
    {
        TimeStamp = lph.TimeStamp;
        if (FirstRun){
            PrevStamp = TimeStamp;
            FirstRun = false;
            return;
        }
        Value += lph.Value / 3600 * ((double)(TimeStamp-PrevStamp)) / 1000;
        PrevStamp = TimeStamp;
        registry.TriggerListeners(this);
    }

    public void SetValue(double value)
    {
        this.Value = value;
    }

}

}
