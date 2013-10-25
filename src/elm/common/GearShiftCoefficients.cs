using System.Collections.Generic;
using hobd.util;

namespace hobd
{
    public class GearShiftCoefficients
    {
        public double MainGear { get; set; }
        public Dictionary<int, Range<double>> Coefficients { get; set; }
    }
}