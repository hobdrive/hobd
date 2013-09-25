using System;
using System.Collections.Generic;

namespace hobd
{

/**
  Improved short term fuel economy sensors.
  Takes into account only non-idle speeds,
  also implement smooth trimming, making the value from 'real' last N seconds.
*/
public class FuelEconomyPeriodSensor : CoreSensor, IAggregatorSensor
{
    public FuelEconomyPeriodSensor(string speed_id, string lph_id) : this(speed_id, lph_id, 5000)
    {
    }

    public FuelEconomyPeriodSensor(string speed_id, string lph_id, int reset_period) : this()
    {
        this.SpeedId = speed_id;
        this.LPHId = lph_id;
        this.ResetPeriod = reset_period;
    }

    public FuelEconomyPeriodSensor()
    {
        ListenInterval = 2000;
        Value = Double.PositiveInfinity;
        this.SpeedId = "Speed";
        this.LPHId = "LitersPerHour";
        Units = "lph";
    }

    Sensor speed, lph;

    string SpeedId;
    string LPHId;

    int ResetPeriod;
    public int ListenInterval {get; set;}
    double IdleSpeed = 5;
    /**
      If we need to reset sensor on idling conditions
    */
    bool FuelEconomyResetOnIdle = false;
    bool FuelEconomySuspendOnIdle = true;

    double[] a_fuel;
    double[] a_distance;

    // current sum
    double t_fuel;
    double t_distance;

    // previous data timestamps
    double p_timestamp;

    // Indexes
    int i_fuel = 0;
    int i_distance = 0;

    bool FirstRunDistance = true;
    bool SuspendCalculations = false;

    protected override void Activate()
    {
        Reset();
        
        try{
            this.IdleSpeed = double.Parse(registry.VehicleParameters["idle-speed"], UnitsConverter.DefaultNumberFormat);
            this.IdleSpeed = double.Parse(registry.VehicleParameters["fuel-economy-idle-speed"], UnitsConverter.DefaultNumberFormat);
        }catch(Exception){}

        try{
            this.FuelEconomyResetOnIdle = registry.VehicleParameters["fuel-economy-reset-on-idle"] == "true";
        }catch(Exception){}
        try{
            this.FuelEconomySuspendOnIdle = registry.VehicleParameters["fuel-economy-suspend-on-idle"] == "true";
        }catch(Exception){}

        speed = registry.Sensor(this.SpeedId);
        lph = registry.Sensor(this.LPHId);
        registry.AddListener(speed, OnChange, ListenInterval);
        registry.AddListener(lph, OnChange, ListenInterval);
    }
    
    protected override void Deactivate()
    {
        registry.RemoveListener(OnChange);
    }

    void OnChange(Sensor s)
    {
        if (s == speed)
        {
            if (s.Value <= IdleSpeed && FuelEconomySuspendOnIdle)
            {
                SuspendCalculations = true;
            }else{
                if (SuspendCalculations && FuelEconomyResetOnIdle)
                    Reset();
                SuspendCalculations = false;
            }
            // no actions on speed sensor
            return;
        }

        TimeStamp = s.TimeStamp;

        if (FirstRunDistance)
        {
            p_timestamp = lph.TimeStamp;
            FirstRunDistance = false;
            return;
        }

        if(!SuspendCalculations)
        {
            int cidx = (int) ((TimeStamp/1000) % a_fuel.Length);
            if (cidx != i_distance)
            {
                t_distance -= a_distance[cidx];
                a_distance[cidx] = 0;
                i_distance = cidx;
            }
            var inc  = speed.Value / 3600 * ((double)(TimeStamp - p_timestamp)) / 1000;
            a_distance[i_distance] += inc;
            t_distance += inc;
            
            if (cidx != i_fuel)
            {
                t_fuel -= a_fuel[cidx];
                a_fuel[cidx] = 0;
                i_fuel = cidx;
            }
            inc = lph.Value * ((double)(TimeStamp - p_timestamp)) / 1000 / 3600;
            a_fuel[i_fuel] += inc;
            t_fuel += inc;

            p_timestamp = TimeStamp;
        }

        if (t_distance <= 0)
        {
            Value = Double.PositiveInfinity;
        }else{
            Value = t_fuel  * 100 / t_distance;
        }
        //System.Console.WriteLine("t_fuel {0}  t_distance {1}", t_fuel, t_distance);
        registry.TriggerListeners(this);
    }

    public virtual void Reset()
    {
        a_fuel = new double[ResetPeriod/1000];
        a_distance = new double[ResetPeriod/1000];
        i_fuel = 0;
        i_distance = 0;
        t_fuel = 0;
        t_distance = 0;
        p_timestamp = 0;
        Value = 0;
        FirstRunDistance = true;
        SuspendCalculations = false;
    }
    public virtual void Suspend()
    {
        FirstRunDistance = true;
        SuspendCalculations = false;
    }

}

}
