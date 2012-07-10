using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using hobd;

namespace hobd.elm.load
{

public class LitersPerHourSensor : CoreSensor
{
    public int ListenInterval{get; set;}

    double engine_load_coeff = 1;
    string default_matrix = "0.025, 0.025, 0.10, 0.20, 0.3, 0.4, 0.3, 0.3, 0.3, 0.3";

    double[] rpm_matrix = null;
    double rpm_step;

    Sensor load, rpm;

    public LitersPerHourSensor()
    {
        ListenInterval = 0;
    }

    public override void SetRegistry(SensorRegistry registry)
    {
        base.SetRegistry(registry);
        
        try{
            this.engine_load_coeff = double.Parse(registry.VehicleParameters["engine-load-coeff"], UnitsConverter.DefaultNumberFormat);
        }catch(Exception){
            Logger.info("LitersPerHourSensor", "Using default coefficient");
        }
        
        if (registry.VehicleParameters.ContainsKey("rpm-consumption-coeff"))
        {
            default_matrix = registry.VehicleParameters["rpm-consumption-coeff"];
        }
        if (default_matrix.Length > 1)
        {
            rpm_matrix = default_matrix
                      .Split(new char[]{',', ' '})
                      .Where(v => v != "")
                      .Select(v => double.Parse(v, UnitsConverter.DefaultNumberFormat)).ToArray();
            rpm_step = 10000 / rpm_matrix.Length;
        }
    }

    protected override void Activate()
    {
        load = registry.Sensor(OBD2Sensors.EngineLoad);
        registry.AddListener(load, OnSensorChange, ListenInterval);
        if (rpm_matrix != null)
        {
            rpm = registry.Sensor(OBD2Sensors.RPM);
            registry.AddListener(rpm, OnSensorChange, ListenInterval);
        }
    }
    
    protected override void Deactivate()
    {
        registry.RemoveListener(OnSensorChange);
    }

    public void OnSensorChange(Sensor s)
    {
        TimeStamp = s.TimeStamp;
        
        int lowidx = (int) (rpm.Value / rpm_step);
        int nextidx = rpm_matrix.Length-1 == lowidx ? lowidx : lowidx + 1;
        var coeff = rpm_matrix[lowidx] +  (rpm_matrix[nextidx]-rpm_matrix[lowidx]) * (rpm.Value - lowidx*rpm_step) / rpm_step;
        Value = load.Value * coeff * engine_load_coeff;

        registry.TriggerListeners(this);
    }

}

}
