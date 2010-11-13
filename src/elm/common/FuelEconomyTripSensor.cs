using System;
using System.Collections.Generic;

namespace hobd
{

public class FuelEconomyTripSensor : CoreSensor
{
    Sensor distance, fuel;
    public int ListenInterval{get; set;}
    public int ResetPeriod{get; set;}
    int cscan = 0;
    double h_fuel = 0;
    double h_distance = 0;

    public FuelEconomyTripSensor()
    {
        ListenInterval = 2000;
        ResetPeriod = 0;
    }

    public override void Activate()
    {
        distance = registry.Sensor(CommonSensors.DistanceRun);
        fuel = registry.Sensor(CommonSensors.FuelConsumed);
        registry.AddListener(distance, OnChange, ListenInterval);
        registry.AddListener(fuel, OnChange, ListenInterval);
    }
    
    public override void Deactivate()
    {
        registry.RemoveListener(OnChange);
    }

    void OnChange(Sensor s)
    {
        TimeStamp = DateTimeMs.Now;
        
        if (s != distance || distance.Value - h_distance == 0 || fuel.Value - h_fuel == 0)
            return;

        Value = (fuel.Value - h_fuel)  * 100 / (distance.Value - h_distance);
        registry.TriggerListeners(this);
        cscan++;
        if (ResetPeriod != 0 && cscan >= ResetPeriod){
            cscan = 0;
            h_fuel = fuel.Value;
            h_distance = distance.Value;
        }
    }

}

}
