using System;
using System.Collections.Generic;

namespace hobd
{

public class FuelEconomyTripSensor : CoreSensor
{
    Sensor distance, fuel;
    public int ListenInterval{get; set;}
        
    public FuelEconomyTripSensor()
    {
        ListenInterval = 2000;
    }

    public override void NotifyAddListener(Action<Sensor> listener)
    {
        base.NotifyAddListener(listener);
        if (listenerCount == 1)
        {
            distance = registry.Sensor(CommonSensors.DistanceRun);
            fuel = registry.Sensor(CommonSensors.FuelConsumed);
            registry.AddListener(distance, OnChange, ListenInterval);
            registry.AddListener(fuel, OnChange, ListenInterval);
        }
    }
    
    public override void NotifyRemoveListener(Action<Sensor> listener)
    {
        base.NotifyRemoveListener(listener);
        if (listenerCount == 0)
        {
            registry.RemoveListener(OnChange);
        }
    }

    void OnChange(Sensor s)
    {
        TimeStamp = DateTimeMs.Now;
        Value = fuel.Value  * 100 / distance.Value;
        registry.TriggerListeners(this);
    }

}

}
