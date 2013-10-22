using System;
using System.Linq;

namespace hobd
{
    public class ShiftPositionSensor : DerivedSensor
    {
        private const double DefaultMainGear = 2.9;
        private double WheelCirc = 1.20576; // default value
        private double mainGear = DefaultMainGear; // default value
        private GearShiftCoefficients _gearShiftCoefficent;

        public ShiftPositionSensor()
            : base(CommonSensors.ShiftPosition, null, null)
        {
            Units = "";
            base.DerivedValue = (speed, rpm) => MapGear(CalculateGearCoefficient(speed, rpm));
        }

        // Speed: KM/H
        // RPM: Rotation speed of the Engine
        private double CalculateGearCoefficient(Sensor speed, Sensor rpm)
        {
            var result = ((rpm.Value / 60)/(speed.Value*1000/3600/WheelCirc))/mainGear;
            return Math.Round(result, 2);
        }

        private double MapGear(double dTmp)
        {
            return _gearShiftCoefficent.GearCoefficients
                                       .Where(coefficient => coefficient.Value.Contains(dTmp))
                                       .Select(c => Convert.ToDouble(c.Key)).Single();
        }

        private bool Validate(Sensor speed, Sensor rpm)
        {
            if ((false == speed.Valid) || (false == rpm.Valid))
            {
                return false;
            }
            if (speed.Value <= 0 || rpm.Value <= 0 || WheelCirc <= 0)
            {

                return false;
            }

            return true;
        }

        protected override void Activate()
        {
            _gearShiftCoefficent = new GearShiftCoefficients(registry);

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
            if (!Validate(speedSensor, rpmSensor))
            {
                Valid = false;
                return;
            }

            var gearCoefficient = CalculateGearCoefficient(speedSensor, rpmSensor);

            Value = MapGear(gearCoefficient);
        }

        public class Range
        {
            public Range(double lowValue, double upValue)
            {
                LowValue = lowValue;
                UpValue = upValue;
            }

            public double UpValue { get;private set; }
            public double LowValue { get;private set; }

            public bool Contains(double value)
            {
                return value >= LowValue && value < UpValue;
            }
        }
    }
}