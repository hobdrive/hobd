using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using hobd;

namespace IntegrationSensorTest
{
   [TestFixture]
    public class IntegrationSensorTest
    {
        private SensorRegistry testRegistry;
        private CoreSensorEx testSpeedSensor;
        private IntegrationSensor testIntegrationSensor;
        AutoResetEvent testEvent;

        [SetUp]
        public void Setup()
        {
            testRegistry =new SensorRegistry();

            testSpeedSensor = new CoreSensorEx("Test", "Speed", "km");
            testIntegrationSensor = new IntegrationSensor("Speed")
                {
                    ID = "IntegrationSensor_test"
                };
            //
            testRegistry.Add(testSpeedSensor);
            testRegistry.Add(testIntegrationSensor);
            //
            testRegistry.AddListener(testIntegrationSensor,OnSensorChange,0);
            //
            testEvent = new AutoResetEvent(false);
        }

        private void OnSensorChange(Sensor s)
        {

        }

        [Test]
        public void TestIntegrationSensorUnlimited()
        {
            const int timeInterval1 = 1000;
            const int timeInterval2 = 2000;
            const int timeInterval3 = 3000;
            const int timeInterval4 = 4000;
            const int timeInterval5 = 5000;

            const double speed1 = 10;
            const double speed2 = 20;
            const double speed3 = 30;
            const double speed4 = 40;
            const double speed5 = 50;

            Assert.AreEqual(testIntegrationSensor.Value, 0);
            
            testSpeedSensor.Update(speed1);
            Thread.Sleep(timeInterval1);
            testSpeedSensor.Update(speed2);
            Thread.Sleep(timeInterval2);
            testSpeedSensor.Update(speed3);
            Thread.Sleep(timeInterval3);
            testSpeedSensor.Update(speed4);
            Thread.Sleep(timeInterval4);
            testSpeedSensor.Update(speed5);

            const double avgSpeed = speed1*(timeInterval1 - 0) + speed2*(timeInterval2 - timeInterval1) +
                                    speed3*(timeInterval3 - timeInterval2) + speed4*(timeInterval4 - timeInterval3) +
                                    speed5*(timeInterval5 - timeInterval4);

            const double avgTimeInterval = (timeInterval1 - 0) + (timeInterval2 - timeInterval1) +
                                           (timeInterval3 - timeInterval2) + (timeInterval4 - timeInterval3) +
                                           (timeInterval5 - timeInterval4);


            Assert.That(testIntegrationSensor.Value, Is.EqualTo(avgSpeed / avgTimeInterval).Within(0.05));
        }

        [Test]
        public void TestIntegrationSensorUnlimitedLoop()
        {
            var avgSpeed = 0;
            var updateCount = 0;
            Assert.AreEqual(testIntegrationSensor.Value, 0);

            for (var i = 10; i < 500; i+=10)
            {
                avgSpeed += i;
                testSpeedSensor.Update(i);
                Thread.Sleep(1000);
                updateCount++;
                Assert.That(testIntegrationSensor.Value, Is.EqualTo((double)avgSpeed / updateCount).Within(0.1));
            }
            Assert.That(testIntegrationSensor.Value, Is.EqualTo(avgSpeed / updateCount).Within(0.1));
        }
    }
}

