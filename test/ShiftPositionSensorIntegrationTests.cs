using System.Diagnostics;
using System.Threading;
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

            ActivateSensor();
        }

        [TestMethod]
        public void Valid_should_be_false_when_rpm_invalid()
        {
            _speedSensor.Update(true, 10);
            _rpmSensor.Update(false, double.NaN);
            Thread.Sleep(100);           //better to use in test one thread (use runtime scheduler)

            Assert.IsFalse(_sensor.Valid);
        }

        [TestMethod]
        public void Valid_should_be_false_when_speed_invalid()
        {
            _rpmSensor.Update(true, 25);
            _speedSensor.Update(false, double.NaN);
            Thread.Sleep(100);

            Assert.IsFalse(_sensor.Valid);
        }

        [TestMethod]
        public void Valid_should_be_true_when_speed_and_rpm_valid()
        {
            _speedSensor.Update(true, 10);
            _rpmSensor.Update(true, 25);
            Thread.Sleep(100);

            Assert.IsTrue(_sensor.Valid);
        }

        private void ActivateSensor()
        {
            _sensor.NotifyAddListener(sensor => Debug.Print(sensor.Value.ToString()));
        }

        [TestMethod]
        public void TestGearShiftPositionSensor()
        {
            // assume we manually trigger "Speed" data
            _speedSensor.Update(100);
            _rpmSensor.Update(3000);
            Thread.Sleep(100);

            Assert.AreEqual(4,_sensor.Value); //According to the Excel sheet "shiftposition.ods"

            _speedSensor.Update(80); // Set new speed value
            _rpmSensor.Update(3000);
            Thread.Sleep(100);

            Assert.AreEqual(3,_sensor.Value);

            _speedSensor.Update(80); // Set new speed value
            _rpmSensor.Update(6000);
            Thread.Sleep(100);

            Assert.AreEqual(_sensor.Value, 2);

            _speedSensor.Update(40); // Set new speed value
            _rpmSensor.Update(6000);
            Thread.Sleep(100);

            Assert.AreEqual(_sensor.Value, 1);

            _speedSensor.Update(10); // Set new speed value
            _rpmSensor.Update(1000);
            Thread.Sleep(100);

            Assert.AreEqual(_sensor.Value, 1);
        }
    }
}
