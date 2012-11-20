using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using hobd;

namespace hobd
{

    public class SpeedCorrectionSensor : DerivedSensor
    {

        public const string PARAM = "speed-correction";
    
        double correction = 1;
    
        public SpeedCorrectionSensor() : base(CommonSensors.Speed, OBD2Sensors.Speed, null)
        {
            ID = "Common.Speed";
            Name = OBD2Sensors.Speed;
            Units = "kph";
            base.DerivedValue = (s,x) => s.Value * correction;
        }
    
        public override void SetRegistry(SensorRegistry registry)
        {
            base.SetRegistry(registry);
            
            var speed = registry.Sensors.FirstOrDefault( s => s.Name == OBD2Sensors.Speed && s.ID != this.ID);
            
            // Force override
            base.aid = speed.ID;
            base.a = speed;
            // This is not necessary, but good?
            this.Units = speed.Units;
            
            try{
                this.correction = double.Parse(registry.VehicleParameters[PARAM], UnitsConverter.DefaultNumberFormat);
            }catch(Exception){
                Logger.info("SpeedCorrectionSensor", "Using default coefficient");
            }
        }
    
    }

}
