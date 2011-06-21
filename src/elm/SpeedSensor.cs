using System;
using System.Collections.Generic;
using System.Globalization;
using hobd;

namespace hobd
{

public class SpeedSensor : OBD2Sensor
{

    double correction = 1;

    public SpeedSensor()
    {
    }

    public override void SetRegistry(SensorRegistry registry)
    {
        base.SetRegistry(registry);
        
        try{
            this.correction = double.Parse(registry.VehicleParameters["speed-correction"], UnitsConverter.DefaultNumberFormat);
        }catch(Exception){
            Logger.info("SpeedSensor", "Using default coefficient");
        }
    }

    public override double Value {
        get{
            return base.Value;
        }
        protected set{
            base.Value = value * correction;
        }
    }

}

}
