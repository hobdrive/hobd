using System.Collections.Generic;

namespace hobd
{
    public class GearShiftCoefficients
    {
        public double MainGear { get; set; }
        public Dictionary<int, ShiftPositionSensor.Range> Coefficients { get; set; }
    }
}