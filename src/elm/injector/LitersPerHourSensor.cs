using System;
using System.Collections.Generic;
using System.Globalization;

namespace hobd.elm.injector
{

/**
 * Calculates liters per hour using injector pulse width sensor
 * and injector efficiency constant
 */
public class LitersPerHourSensor : CoreSensor
{
    public int ListenInterval{get; set;}
    int cylinders = 0;
    double injectorccpm = 0;
    Sensor ipw, rpm;
        
    public LitersPerHourSensor()
    {
        ListenInterval = 0;
    }

    public override void SetRegistry(SensorRegistry registry)
    {
        base.SetRegistry(registry);
        
        this.cylinders = int.Parse(registry.VehicleParameters["cylinders"]);
        this.injectorccpm = double.Parse(registry.VehicleParameters["injector-ccpm"], UnitsConverter.DefaultNumberFormat);
    }

    public override void Activate()
    {
        ipw = registry.Sensor("InjectorPulseWidth");
        rpm = registry.Sensor("RPM");
        registry.AddListener(ipw, OnSensorChange, ListenInterval);
        registry.AddListener(rpm, OnSensorChange, ListenInterval);
    }
    
    public override void Deactivate()
    {
        registry.RemoveListener(OnSensorChange);
    }

	/**
	 * Calculates as following:
	 *
	 * rpm/60 * cilinders * injector * 0.001 * injectorFlow * 60 / 1000
	 *
	 * rpm/60 is rotations per second
	 * cilinders number of cilinders
	 * injector*0.001 is how long injector is open during one rotation (in seconds)
	 * injectorFlow how much cubic centimeters (CC) come through the injector in 1 minute
	 * /60 is to give an second
	 * /1000 is to give liters from CC
	 * 
	 */
    public void OnSensorChange(Sensor s)
    {
        TimeStamp = s.TimeStamp;
        // liters per second
        Value = rpm.Value/60 * cylinders * ipw.Value * 0.001 * injectorccpm/60 / 1000;
        // to hour
        Value = Value*3600;
        registry.TriggerListeners(this);
    }

}

}
