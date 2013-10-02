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
            double dGear;
            double dTmp;
            if ((false == speed.Valid) || (false == rpm.Valid))
            {
                return 0;
            }
            if (speed.Value <= 0 || rpm.Value <= 0 || WheelCirc <= 0)
            {
                return 0;
            }
            
            dTmp = ((rpm.Value / 60) / (speed.Value * 1000 / 3600 / WheelCirc)) / mainGear;
            dTmp = Math.Round(dTmp, 2);
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
