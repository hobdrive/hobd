using System;
using System.Collections.Generic;

namespace hobd
{
    /// <summary>
    /// Simple coresensor with ability to be updated from external logic
    /// </summary>
    public class CoreSensorEx : CoreSensor
    {
    
        public CoreSensorEx(string NS, string Name, string Units)
        {
            this.Name  = Name;
            this.ID    = NS+"."+Name;
            this.Units = Units;
        }
    
        public void Update(double value)
        {
            this.Update(true, value);
        }

        public void Update(bool valid, double value)
        {
            this.Valid = valid;
            this.Value = value;
            this.TimeStamp = DateTimeMs.Now;
            registry.TriggerListeners(this);
        }

    }

}
