using System;
using System.Linq;

namespace hobd
{
    public class ShiftPositionSensor : DerivedSensor
    {
        private double WheelCirc = 1.20576; // default value
        public const string GearShiftParameterName = "gearshift-coefficients";
        private readonly GearShiftCoefficientsParser _gearShiftCoefficentParser;
        private GearShiftCoefficients _gearShiftCoefficients;
        public ShiftPositionSensor()
            : base(CommonSensors.ShiftPosition, null, null)
        {
            _gearShiftCoefficentParser = new GearShiftCoefficientsParser();
            Units = "";
            base.DerivedValue = (speed, rpm) => MapGear(CalculateGearCoefficient(speed, rpm));
        }

        // Speed: KM/H
        // RPM: Rotation speed of the Engine
        private double CalculateGearCoefficient(Sensor speed, Sensor rpm)
        {
            var result = ((rpm.Value / 60) / (speed.Value * 1000 / 3600 / WheelCirc)) / _gearShiftCoefficients.MainGear;
            return Math.Round(result, 2);
        }

        private double MapGear(double dTmp)
        {
            return _gearShiftCoefficients.Coefficients
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
            if (registry.VehicleParameters.ContainsKey(GearShiftParameterName))
            {
                var coefficientSource = registry.VehicleParameters[GearShiftParameterName];
                _gearShiftCoefficients = _gearShiftCoefficentParser.Parse(coefficientSource);
            }
            else
            {
                _gearShiftCoefficients = _gearShiftCoefficentParser.GetDefault();
            }

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