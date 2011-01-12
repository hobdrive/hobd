using System;
using System.Collections.Generic;
using System.Globalization;
using hobd;

namespace hobd.elm.load
{

public class LitersPerHourSensor : CoreSensor
{
    public int ListenInterval{get; set;}

    double load_consumption_coeff = 14.7;

    Sensor load;

    public LitersPerHourSensor()
    {
        ListenInterval = 0;
    }

    public override void SetRegistry(SensorRegistry registry)
    {
        base.SetRegistry(registry);
        
        try{
            this.load_consumption_coeff = double.Parse(registry.VehicleParameters["load-consumption-coeff"], UnitsConverter.DefaultNumberFormat);
        }catch(Exception e){
            Logger.info("LitersPerHourSensor", "Using default coefficient", e);
        }
    }

    public override void Activate()
    {
        load = registry.Sensor(OBD2Sensors.EngineLoad);
        registry.AddListener(load, OnSensorChange, ListenInterval);
    }
    
    public override void Deactivate()
    {
        registry.RemoveListener(OnSensorChange);
    }

    public void OnSensorChange(Sensor s)
    {
        TimeStamp = DateTimeMs.Now;
        Value = load.Value * load_consumption_coeff;
        registry.TriggerListeners(this);
    }

}

}
