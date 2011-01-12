using System;
using System.Collections.Generic;
using System.Globalization;
using hobd;

namespace hobd.elm.map
{

public class LitersPerHourSensor : CoreSensor
{
    public int ListenInterval{get; set;}

    double stoich = 14.7;
    double displacement = 1.8;
    double ve = 95;

    Sensor rpm, map, iat;
        
    public LitersPerHourSensor()
    {
        ListenInterval = 0;
    }

    public override void SetRegistry(SensorRegistry registry)
    {
        base.SetRegistry(registry);
        
        try{
            this.stoich = double.Parse(registry.VehicleParameters["stoich"], UnitsConverter.DefaultNumberFormat);
            this.displacement = double.Parse(registry.VehicleParameters["liters"], UnitsConverter.DefaultNumberFormat);
            this.ve = double.Parse(registry.VehicleParameters["volumetric-efficiency"], UnitsConverter.DefaultNumberFormat);

        }catch(Exception e){
            Logger.info("LitersPerHourSensor", "Using default displacement and VE ratio", e);
        }
    }

    public override void Activate()
    {
        map = registry.Sensor(OBD2Sensors.IntakeManifoldPressure);
        registry.AddListener(map, OnSensorChange, ListenInterval);
        
        rpm = registry.Sensor(OBD2Sensors.RPM);
        registry.AddListener(rpm, OnSensorChange, ListenInterval);

        iat = registry.Sensor(OBD2Sensors.IntakeAirTemp);
        registry.AddListener(iat, OnSensorChange, 3000 + ListenInterval);
    }
    
    public override void Deactivate()
    {
        registry.RemoveListener(OnSensorChange);
    }

	/**
	 * Calculates as following:
	   (credits to ECUTracker)

       IMAP = RPM * MAP / IAT
       MAF = (IMAP/120)*(VE/100)*(ED)*(MM)/(R)

       RPM - Revs per Minute
       MAP - Pressure in kPa
       IAT degrees Kelvin
       R - 8.314 J/K/mole
       MM - Average Molecular Mass of Air 28.97 g/mole.
       VE - Percentage Volumetric Efficiency
       ED - Engine Displacement in Liters
	 *
	 * 
	 */
    public void OnSensorChange(Sensor s)
    {
        TimeStamp = DateTimeMs.Now;
        // per second
        double imap = rpm.Value * map.Value / (iat.Value + 273.15);
        double maf = imap/120 * (this.ve/100) * this.displacement * (28.97) / 8.314;

        // to hour
        Value = maf / (stoich * 454*6.17 / 3.78 ) * 3600;
        registry.TriggerListeners(this);
    }

}

}
