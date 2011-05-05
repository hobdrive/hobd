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

    double load_consumption_coeff = 0.02;
    string default_matrix = "0.010, 0.011, 0.025, 0.050, 0.055, 0.06, 0.06, 0.06, 0.06, 0.06";

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
            this.load_consumption_coeff = double.Parse(registry.VehicleParameters["load-consumption-coeff"], UnitsConverter.DefaultNumberFormat);
        }catch(Exception e){
            Logger.info("LitersPerHourSensor", "Using default coefficient", e);
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

    public override void Activate()
    {
        load = registry.Sensor(OBD2Sensors.EngineLoad);
        registry.AddListener(load, OnSensorChange, ListenInterval);
        if (rpm_matrix != null)
        {
            rpm = registry.Sensor(OBD2Sensors.RPM);
            registry.AddListener(rpm, OnSensorChange, ListenInterval);
        }
    }
    
    public override void Deactivate()
    {
        registry.RemoveListener(OnSensorChange);
    }

    public void OnSensorChange(Sensor s)
    {
        TimeStamp = s.TimeStamp;
        if (rpm_matrix != null)
        {
            int lowidx = (int) (rpm.Value / rpm_step);
            int nextidx = rpm_matrix.Length-1 == lowidx ? lowidx : lowidx + 1;
            var coeff = rpm_matrix[lowidx] +  (rpm_matrix[nextidx]-rpm_matrix[lowidx]) * (rpm.Value - lowidx*rpm_step) / rpm_step;
            Value = load.Value * coeff;
        }else
            Value = load.Value * load_consumption_coeff;
        registry.TriggerListeners(this);
    }

}

}
