using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hobd
{
    public class ShiftPositionSensor : DerivedSensor
    {
        private double WheelCirc = 1.20576;// default value
        private double mainGear = 2.9;// default value

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
            if ((false == speed.Valid) || (false == rpm.Valid))
            {
                return Double.PositiveInfinity;
            }
            if (speed.Value <= 0 || rpm.Value <= 0 || WheelCirc <= 0)
            {
                return Double.PositiveInfinity;
            }
            else
            {
                return ((rpm.Value / 60) / (speed.Value * 1000 / 3600 / WheelCirc)) / mainGear;
            }
        }

        public double WheelCircleLength
        {
            get
            {
                return this.WheelCirc;
            }
            set
            {
                this.WheelCirc = value;
            }
        }

        public double MainGear
        {
            get
            {
                return this.mainGear;
            }
            set
            {
                this.mainGear = value;
            }
        }
    }
}
