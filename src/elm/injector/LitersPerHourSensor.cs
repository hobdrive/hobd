using System;
using System.Collections.Generic;
using System.Globalization;

namespace hobd
{

/**
 * Calculates total run distance based on speed interpolation
 */
public class LitersPerHourSensor : CoreSensor
{
    public int ListenInterval{get; set;}
    int cylinders = 0;
    double injectorccpm = 0;
    Sensor ipw, rpm;
        
    public LitersPerHourSensor()
    {
        ID = "LitersPerHour";
        Name = "Liters Per Hour";
        Description = "Liter Per Hour (injector based)";
        Units = "l/h";
        ListenInterval = 0;
    }

    public override void SetRegistry(SensorRegistry registry)
    {
        base.SetRegistry(registry);
        
        this.cylinders = int.Parse(registry.VehicleParameters["cylinders"]);
        this.injectorccpm = double.Parse(registry.VehicleParameters["injector-ccpm"], HOBD.DefaultNumberFormat);
        Logger.trace("LitersPerHour", "cyl:"+this.cylinders + "ccpm:"+this.injectorccpm);
    }

    public override void NotifyAddListener(Action<Sensor> listener)
    {
        base.NotifyAddListener(listener);
        if (listenerCount == 1)
        {
            Logger.trace("LitersPerHour", "Register");
            ipw = registry.Sensor("InjectorPulseWidth");
            rpm = registry.Sensor("RPM");
            registry.AddListener(ipw, OnSensorChange, ListenInterval);
            registry.AddListener(rpm, OnSensorChange, ListenInterval);
        }
    }
    
    public override void NotifyRemoveListener(Action<Sensor> listener)
    {
        base.NotifyRemoveListener(listener);
        if (listenerCount == 0)
        {
            Logger.trace("LitersPerHour", "Unregister");
            registry.RemoveListener(OnSensorChange);
        }
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
        Logger.trace("LitersPerHour", "rpm:"+rpm.Value + "ipw:"+ipw.Value);
        // liters per second
        Value = rpm.Value/60 * cylinders * ipw.Value * 0.001 * injectorccpm/60 / 1000;
        // to hour
        Value = Value*3600;
        registry.TriggerListeners(this);
    }

}

}
