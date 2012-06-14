using System;
using System.Collections.Generic;

namespace hobd
{

public class FuelEconomyTripSensor : DerivedSensor
{
    public FuelEconomyTripSensor(string distance_id, string fuel_id) : base("", distance_id, fuel_id, true, false, 2000)
    {
        Value = Double.PositiveInfinity;
        base.DerivedValue = FEValue;
        Units = "lph";
    }

    public FuelEconomyTripSensor() : this("DistanceRun", "FuelConsumed")
    {
    }
    
    double FEValue(Sensor distance, Sensor fuel)
    {
        if (distance.Value <= 0 || fuel.Value <= 0)
        {
            return Double.PositiveInfinity;
        }else{
            return fuel.Value * 100 / distance.Value;
        }
    }

}

}
