using Microsoft.VisualStudio.TestTools.UnitTesting;
using hobd;

namespace hobdCoreTest
{
    [TestClass]
    public class GearShiftCoefficientsParserTests
    {
        private GearShiftCoefficientsParser _coefficientsParser;
        private const string DefaultCoefficientValue = "2.962; 3.643; 2.008; 1.296; 0.892";

        [TestInitialize]
        public void SetUp()
        {
            _coefficientsParser = new GearShiftCoefficientsParser();
        }

        [TestMethod]
        public void should_return_default_coefficient_when_not_exists()
        {
            var defaultCoefficients = _coefficientsParser.GetDefault();
            Assert.AreEqual(defaultCoefficients.MainGear, 2.9);

            Assert.AreEqual(defaultCoefficients.Coefficients.Count, 4);
        }

        [TestMethod]
        public void should_return_MainGear()
        {
            var coefficients = _coefficientsParser.Parse(DefaultCoefficientValue);

            Assert.AreEqual(coefficients.MainGear, 2.962);
        }

        [TestMethod]
        public void should_return_Coefficients()
        {
            var coefficients = _coefficientsParser.Parse(DefaultCoefficientValue).Coefficients;
            Assert.AreEqual(0, coefficients[5].Low);
            Assert.AreEqual(0.892, coefficients[5].Up);

            Assert.AreEqual(0.892, coefficients[4].Low);
            Assert.AreEqual(1.296, coefficients[4].Up);

            Assert.AreEqual(1.296, coefficients[3].Low);
            Assert.AreEqual(2.008, coefficients[3].Up);

            Assert.AreEqual(2.008, coefficients[2].Low);
            Assert.AreEqual(3.643, coefficients[2].Up);

            Assert.AreEqual(3.643, coefficients[1].Low);
            Assert.AreEqual(double.MaxValue, coefficients[1].Up);
        }
    }
}