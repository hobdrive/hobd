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
        private SensorRegistry _testRegistry;
        private CoreSensorEx _testSpeedSensor;
        private IntegrationSensor _testIntegrationSensorUnlimited;
        private IntegrationSensor _testIntegrationSensorLimited;

        [SetUp]
        public void Setup()
        {
            _testRegistry =new SensorRegistry();

            _testSpeedSensor = new CoreSensorEx("Test", "Speed", "km");
            _testIntegrationSensorUnlimited = new IntegrationSensor("Speed")
                {
                    Name = "IntegrationSensorTestUnlimited",
                    ID = "IntegrationSensorTestUnlimitedId"
                };
            _testIntegrationSensorLimited = new IntegrationSensor("Speed", 5000)
            {
                Name = "IntegrationSensorTestLimited",
                ID = "IntegrationSensorTestLimitedId"
            };
            //
            _testRegistry.Add(_testSpeedSensor);
            _testRegistry.Add(_testIntegrationSensorUnlimited);
            _testRegistry.Add(_testIntegrationSensorLimited);
            //
            _testRegistry.AddListener(_testIntegrationSensorUnlimited, OnUnlimitedSensorChange,0);
            _testRegistry.AddListener(_testIntegrationSensorLimited, OnLimitedSensorChange, 0);
            //
        }

        private void OnUnlimitedSensorChange(Sensor s)
        {

        }

        private void OnLimitedSensorChange(Sensor s)
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

            Assert.AreEqual(_testIntegrationSensorUnlimited.Value, 0);
            
            _testSpeedSensor.Update(speed1);
            Thread.Sleep(timeInterval1);
            _testSpeedSensor.Update(speed2);
            Thread.Sleep(timeInterval2);
            _testSpeedSensor.Update(speed3);
            Thread.Sleep(timeInterval3);
            _testSpeedSensor.Update(speed4);
            Thread.Sleep(timeInterval4);
            _testSpeedSensor.Update(speed5);

            const double avgSpeed = speed1*(timeInterval1 - 0) + speed2*(timeInterval2 - timeInterval1) +
                                    speed3*(timeInterval3 - timeInterval2) + speed4*(timeInterval4 - timeInterval3) +
                                    speed5*(timeInterval5 - timeInterval4);

            const double avgTimeInterval = (timeInterval1 - 0) + (timeInterval2 - timeInterval1) +
                                           (timeInterval3 - timeInterval2) + (timeInterval4 - timeInterval3) +
                                           (timeInterval5 - timeInterval4);


            Assert.That(_testIntegrationSensorUnlimited.Value, Is.EqualTo(avgSpeed / avgTimeInterval).Within(0.05));
        }

        [Test]
        public void TestIntegrationSensorUnlimitedLoop()
        {
            var avgSpeed = 0;
            var updateCount = 0;
            Assert.AreEqual(_testIntegrationSensorUnlimited.Value, 0);

            for (var i = 10; i < 500; i+=10)
            {
                avgSpeed += i;
                _testSpeedSensor.Update(i);
                Thread.Sleep(1000);
                updateCount++;
                Assert.That(_testIntegrationSensorUnlimited.Value, Is.EqualTo((double)avgSpeed / updateCount).Within(0.1));
            }
            Assert.That(_testIntegrationSensorUnlimited.Value, Is.EqualTo(avgSpeed / updateCount).Within(0.1));
        }

        [Test]
        public void TestIntegrationSensorLimitedLoop()
        {
            Assert.AreEqual(_testIntegrationSensorLimited.Value, 0);

            _testSpeedSensor.Update(0);
            Thread.Sleep(1000);
            _testSpeedSensor.Update(20);
            Thread.Sleep(4000);
            //_testSpeedSensor.Update(0);
            
            Assert.AreEqual(20,_testIntegrationSensorLimited.Value / _testIntegrationSensorLimited.TotalTime);

            //
            _testSpeedSensor.Update(0);
            Thread.Sleep(1000);
            //
            for (var i = 1; i <= 100; i ++)
            {
                _testSpeedSensor.Update(i);
                Thread.Sleep(1000);
                Assert.That(_testIntegrationSensorLimited.Value / _testIntegrationSensorLimited.TotalTime, Is.EqualTo(10).Within(0.01));
            }
        }
    }
}

