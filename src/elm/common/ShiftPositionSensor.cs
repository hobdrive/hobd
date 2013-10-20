using System;
using System.Linq;

namespace hobd
{
    public class ShiftPositionSensor : DerivedSensor
    {
        private double WheelCirc = 1.20576; // default value
        private double mainGear = 2.9; // default value

        public ShiftPositionSensor()
            : base(CommonSensors.ShiftPosition, null, null)
        {
            ID = "Common.ShiftPosition";
            Units = "";

            base.DerivedValue = GetShiftPos;
        }

        // Speed: KM/H
        // RPM: Rotation speed of the Engine
        public double GetShiftPos(Sensor speed, Sensor rpm)
        {
            if (!Validate(speed, rpm))
                return 0;

            var gearCoefficient = CalculateGearCoefficient(speed, rpm);

            return MapGear(gearCoefficient);
        }

        private double CalculateGearCoefficient(Sensor speed, Sensor rpm)
        {
            var result = ((rpm.Value / 60) / (speed.Value * 1000 /3600 / WheelCirc)) / mainGear;
            result = Math.Round(result, 2);
            return result;
        }

        private static double MapGear(double dTmp)
        {
            double dGear;
            if (dTmp < 0.94)
            {
                dGear = 4;
            }
            else if ((dTmp >= 0.94) && (dTmp < 1.87))
            {
                dGear = 3;
            }
            else if ((dTmp >= 1.87) && (dTmp < 2.49))
            {
                dGear = 2;
            }
            else
            {
                dGear = 1;
            }
            return dGear;
        }

        private bool Validate(Sensor speed, Sensor rpm)
        {
            if ((false == speed.Valid) || (false == rpm.Valid))
            {
                this.Valid = false;
                {
                    value = 0;
                    return (Valid = false);
                }
            }
            if (speed.Value <= 0 || rpm.Value <= 0 || WheelCirc <= 0)
            {
                this.Valid = false;
                {
                    value = 0;
                    return (Valid = false);
                }
            }

            return Valid = true;
        }

        protected override void Activate()
        {
            var speedSensor = GetSpeedSensor();
            if (speedSensor == null)
            {
                Valid = false;
                return;
            }

            var rpmSensor = GetRpmSensor();
            if (rpmSensor == null)
            {
                Valid = false;
                return;
            }

            registry.AddListener(speedSensor, OnSpeedChanged);
            registry.AddListener(rpmSensor, OnRpmChanged);
         
            CalculateGear(speedSensor, rpmSensor);
        }

        private Sensor GetSpeedSensor()
        {
            return registry.Sensors.FirstOrDefault(speed => speed.ID == CommonSensors.Speed);
        }

        private Sensor GetRpmSensor()
        {
            return registry.Sensors.FirstOrDefault(rpm => rpm.ID == CommonSensors.Rpm);
        }

        private void OnRpmChanged(Sensor rpmSensor)
        {
            CalculateGear(GetSpeedSensor(), rpmSensor);
        }

        private void OnSpeedChanged(Sensor speedSensor)
        {
            CalculateGear(speedSensor, GetRpmSensor());
        }

        private void CalculateGear(Sensor speedSensor, Sensor rpmSensor)
        {
            Value = GetShiftPos(speedSensor, rpmSensor);
        }
    }
}