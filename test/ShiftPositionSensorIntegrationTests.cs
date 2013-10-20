using System.Diagnostics;
using hobd;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace hobdCoreTest
{
    [TestClass]
    public class ShiftPositionSensorIntegrationTests
    {
        private CoreSensorEx _speedSensor;
        private SensorRegistry _registry;
        private CoreSensorEx _rpmSensor;
        private ShiftPositionSensor _sensor;

        [TestInitialize]
        public void SetUp()
        {
            _registry = new SensorRegistry();
         
            _speedSensor = new CoreSensorEx("Test", "Speed", "km/h"){ID = CommonSensors.Speed};
            _registry.Add(_speedSensor);

            _rpmSensor = new CoreSensorEx("Test", "RPM", ""){ID = CommonSensors.Rpm};
            _registry.Add(_rpmSensor);

            _sensor = new ShiftPositionSensor();
            _registry.Add(_sensor);
        
        }

        [TestMethod]
        public void Valid_should_be_false_when_rpm_invalid()
        {
            _rpmSensor.Update(false, double.NaN);

            Assert.IsFalse(_sensor.Valid);
        }

        [TestMethod]
        public void Valid_should_be_false_when_speed_invalid()
        {
            _speedSensor.Update(false, double.NaN);

            Assert.IsFalse(_sensor.Valid);
        }

        [TestMethod]
        public void Valid_should_be_true_when_speed_and_rpm_valid()
        {
            _speedSensor.Update(true, 10);
            _rpmSensor.Update(true, 25);

            ActivateSensor();

            Assert.IsTrue(_sensor.Valid);
        }

        private void ActivateSensor()
        {
            _sensor.NotifyAddListener(sensor => Debug.Print(sensor.Value.ToString()));
        }

        [TestMethod]
        public void TestGearShiftPositionSensor()
        {
            double tmpResult = 0.0;
            var Registry = new SensorRegistry();

            // test sensor
            var speedSensor = new CoreSensorEx("Test", "Speed", "km/h");
            speedSensor.SetRegistry(Registry);
            var rpmSensor = new CoreSensorEx("Test", "RPM", "");
            rpmSensor.SetRegistry(Registry);
            var distanceSensor = new ShiftPositionSensor();

            // This should be done better - with an array of predefined values for Speed, RPM and expected result

            // assume we manually trigger "Speed" data
            speedSensor.Update(100);
            rpmSensor.Update(3000);
            tmpResult = distanceSensor.GetShiftPos(speedSensor, rpmSensor);
            Assert.AreEqual(tmpResult, 4);       //According to the Excel sheet "shiftposition.ods"

            speedSensor.Update(80);// Set new speed value
            rpmSensor.Update(3000);
            tmpResult = distanceSensor.GetShiftPos(speedSensor, rpmSensor);
            Assert.AreEqual(tmpResult, 3);

            speedSensor.Update(80);// Set new speed value
            rpmSensor.Update(6000);
            tmpResult = distanceSensor.GetShiftPos(speedSensor, rpmSensor);
            Assert.AreEqual(tmpResult, 2);

            speedSensor.Update(40);// Set new speed value
            rpmSensor.Update(6000);
            tmpResult = distanceSensor.GetShiftPos(speedSensor, rpmSensor);
            Assert.AreEqual(tmpResult, 1);

            speedSensor.Update(10);// Set new speed value
            rpmSensor.Update(1000);
            tmpResult = distanceSensor.GetShiftPos(speedSensor, rpmSensor);
            Assert.AreEqual(tmpResult, 1);
        }
    }
}
