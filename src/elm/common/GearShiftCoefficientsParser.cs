using System.Collections.Generic;
using System.Linq;

namespace hobd
{
    public class GearShiftCoefficientsParser
    {
        private const double DefaultMainGear = 2.9;
        private readonly Dictionary<int, ShiftPositionSensor.Range> _defaultCoefficients = new Dictionary<int, ShiftPositionSensor.Range>();
        private const char CoefficientSeparator = ';';

        public GearShiftCoefficientsParser()
        {
            _defaultCoefficients.Add(1, new ShiftPositionSensor.Range(2.49, double.MaxValue));
            _defaultCoefficients.Add(2, new ShiftPositionSensor.Range(1.87, 2.49));
            _defaultCoefficients.Add(3, new ShiftPositionSensor.Range(0.94, 1.87));
            _defaultCoefficients.Add(4, new ShiftPositionSensor.Range(0, 0.94));
        }

        public GearShiftCoefficients Parse(string sourceCoefficients)
        {
            if (string.IsNullOrEmpty(sourceCoefficients))
                return GetDefault();

            return new GearShiftCoefficients()
                       {
                           Coefficients = ParseCoefficients(sourceCoefficients),
                           MainGear = ParseMainGear(sourceCoefficients)
                       };
        }

        public GearShiftCoefficients GetDefault()
        {
            return new GearShiftCoefficients()
                       {
                           MainGear = DefaultMainGear,
                           Coefficients = _defaultCoefficients
                       };
        }

        private static Dictionary<int, ShiftPositionSensor.Range> ParseCoefficients(string coefficientString)
        {
            var coefficientList = coefficientString.Split(CoefficientSeparator)
                                                   .Skip(1)
                                                   .Reverse()
                                                   .Select(str => double.Parse(str, UnitsConverter.DefaultNumberFormat))
                                                   .ToArray();

            return MapToGearShiftCoefficientDictionary(coefficientList);
        }

        private static Dictionary<int, ShiftPositionSensor.Range> MapToGearShiftCoefficientDictionary(double[] coefficientList)
        {
            var result = new Dictionary<int, ShiftPositionSensor.Range>();

            var lastGear = coefficientList.Length+1;
            var range = new ShiftPositionSensor.Range(0, coefficientList.First());

            result.Add(lastGear, range);

            for (var i = 1; i < coefficientList.Length; i++)
            {
                var gearNumber = coefficientList.Length-i+1;
                var currentRange = new ShiftPositionSensor.Range(coefficientList[i - 1], coefficientList[i]);

                result.Add(gearNumber, currentRange);
            }

            result.Add(1, new ShiftPositionSensor.Range(coefficientList.Last(), double.MaxValue));
            return result;
        }

        private static double ParseMainGear(string vehicleParameter)
        {
            var mainGearStr = vehicleParameter.Split(CoefficientSeparator)[0];
            return double.Parse(mainGearStr, UnitsConverter.DefaultNumberFormat);
        }
    }
}