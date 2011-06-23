using System;
using System.Collections.Generic;

namespace hobd
{

public class FuelEconomyTripSensor : CoreSensor
{
    Sensor distance, fuel;
    string DistanceId;
    string FuelId;

    public int ListenInterval{get; set;}

    public FuelEconomyTripSensor(string distance_id, string fuel_id) : this()
    {
        this.DistanceId = distance_id;
        this.FuelId = fuel_id;
    }

    int cscan = 0;

    public FuelEconomyTripSensor()
    {
        ListenInterval = 2000;
        Value = Double.PositiveInfinity;
        DistanceId = "DistanceRun";
        FuelId = "FuelConsumed";
        Units = "lph";
    }

    public override void Activate()
    {
        distance = registry.Sensor(this.DistanceId);
        fuel = registry.Sensor(this.FuelId);
        registry.AddListener(distance, OnChange, ListenInterval);
        registry.AddListener(fuel, OnChange, ListenInterval);
    }
    
    public override void Deactivate()
    {
        registry.RemoveListener(OnChange);
    }

    void OnChange(Sensor s)
    {
        TimeStamp = s.TimeStamp;
        
        if (distance.Value <= 0 || fuel.Value <= 0)
        {
            Value = Double.PositiveInfinity;
        }else{
            Value = fuel.Value * 100 / distance.Value;
        }
        if (s != distance)
            return;
        registry.TriggerListeners(this);
        cscan++;
    }

}

}
