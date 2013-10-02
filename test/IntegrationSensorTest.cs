using System;
using System.Threading;
using hobd;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace hobdCoreTest
{
    [TestClass]
    public class IntegrationSensorTest
    {
        

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
