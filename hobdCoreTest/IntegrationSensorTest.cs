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
        public void TestSpeedSensorIntegration()
        {

            var Registry = new SensorRegistry();
            
            // test sensor
            var speedSensor = new CoreSensorEx("Test","Speed", "km");
            speedSensor.SetRegistry(Registry);
            var distanceSensor = new IntegrationSensor("Speed");


            // assume we manually trigger "Speed" data
            speedSensor.Update(0);
            Thread.Sleep(1000);
            speedSensor.Update(10);
            Thread.Sleep(1000);
            speedSensor.Update(20);
            Assert.AreEqual(distanceSensor.Value, 10 + 20*2);       //magic numbers!
        }

        public void TestDistanceIntegrationSensor()
        {
            // distance we passed during the last 60 seconds
            var distanceSensor = new IntegrationSensor("Speed", 60);
            //distanceSensor.;
        }
    }
}
