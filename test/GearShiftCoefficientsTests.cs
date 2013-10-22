using Microsoft.VisualStudio.TestTools.UnitTesting;
using hobd;

namespace hobdCoreTest
{
    [TestClass]
    public class GearShiftCoefficientsTests
    {
        private SensorRegistry _registry;
        private GearShiftCoefficients _coefficients;
        private const string DefaultCoefficientValue = "2.962; 3.643; 2.008; 1.296; 0.892";

        [TestInitialize]
        public void SetUp()
        {
            _registry = new SensorRegistry();
            _coefficients = new GearShiftCoefficients(_registry);
        }

        [TestMethod]
        public void should_return_default_coefficient_when_not_exists()
        {
            Assert.AreEqual(_coefficients.MainGear, 2.9);

            Assert.AreEqual(_coefficients.GearCoefficients.Count, 4);
        }

        [TestMethod]
        public void should_return_MainGear()
        {
            _registry.VehicleParameters.Add(GearShiftCoefficients.GearShiftParameterName, DefaultCoefficientValue);

            Assert.AreEqual(_coefficients.MainGear, 2.962);
        }

        [TestMethod]
        public void should_return_Coefficients()
        {
            _registry.VehicleParameters.Add(GearShiftCoefficients.GearShiftParameterName, DefaultCoefficientValue);

            var coefficients = _coefficients.GearCoefficients;
            Assert.AreEqual(0, coefficients[5].LowValue);
            Assert.AreEqual(0.892,coefficients[5].UpValue);

            Assert.AreEqual(0.892,coefficients[4].LowValue);
            Assert.AreEqual(1.296, coefficients[4].UpValue);

            Assert.AreEqual(1.296, coefficients[3].LowValue);
            Assert.AreEqual(2.008, coefficients[3].UpValue);

            Assert.AreEqual(2.008, coefficients[2].LowValue);
            Assert.AreEqual(3.643, coefficients[2].UpValue);

            Assert.AreEqual(3.643, coefficients[1].LowValue);
            Assert.AreEqual(double.MaxValue, coefficients[1].UpValue);
        }
    }
}