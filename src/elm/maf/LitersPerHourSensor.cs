using System;
using System.Collections.Generic;
using System.Globalization;
using hobd;

namespace hobd.elm.maf
{

public class LitersPerHourSensor : CoreSensor
{
    public int ListenInterval{get; set;}
    double stoich = 14.7;
        
    public LitersPerHourSensor()
    {
        ListenInterval = 0;
    }

    public override void SetRegistry(SensorRegistry registry)
    {
        base.SetRegistry(registry);
        
        try{
            this.stoich = double.Parse(registry.VehicleParameters["stoich"], HOBD.DefaultNumberFormat);
        }catch(Exception e){
            Logger.info("LitersPerHourSensor", "Using default stoich ratio", e);
        }
    }

    public override void Activate()
    {
        registry.AddListener("MAF", OnSensorChange, ListenInterval);
    }
    
    public override void Deactivate()
    {
        registry.RemoveListener(OnSensorChange);
    }

	/**
	 * Calculates as following:
	 *
       LPH = 3600 * maf /  (14.7 * 454*6.17 / 3.78 )
       
       14.7 grams of air to 1 gram of gasoline - ideal air/fuel ratio
       
       6.17 pounds per gallon - density of gasoline
       454 grams per pound - conversion
       454*6.17/3.78 - grams per liter

       3600 seconds per hour - conversion
       MAF - mass air flow rate in grams per second
	 * 
	 */
    public void OnSensorChange(Sensor maf)
    {
        TimeStamp = DateTimeMs.Now;
        // per second
        Value = maf.Value / (stoich * 454*6.17 / 3.78 );
        // to hour
        Value = Value*3600;
        registry.TriggerListeners(this);
    }

}

}
