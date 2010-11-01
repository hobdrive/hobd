using System;
using System.Collections.Generic;

namespace hobd
{

public class FuelEconomyTripSensor : CoreSensor
{
    public static string EconomyUnits = "l/100km";
    Sensor distance, fuel;
    public int ListenInterval{get; set;}
        
    public FuelEconomyTripSensor()
    {
        ID = "FuelEconomy_trip";
        Name = "Fuel Economy";
        Description = "Total fuel economy on trip";
        Units = FuelEconomyTripSensor.EconomyUnits;
        ListenInterval = 2000;
    }

    public override void NotifyAddListener(Action<Sensor> listener)
    {
        base.NotifyAddListener(listener);
        if (listenerCount == 1)
        {
            distance = registry.Sensor("DistanceRun");
            fuel = registry.Sensor("FuelConsumed");
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
        Value = fuel.Value / distance.Value * 100000;
        registry.TriggerListeners(this);
    }

}

}
