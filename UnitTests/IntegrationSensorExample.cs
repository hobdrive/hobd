using System;
using System.Threading;
using hobd;
using NUnit.Framework;
using hobd.src.elm.common;

namespace IntegrationSensorTest
{
    internal class IntegrationSensorExample
    {
        static long _elapsedTime;

        private static void Main(string[] args)
        {
            const int timeMs = 1000;
            var registry = new SensorRegistry();

            var speedSensor = new CoreSensorEx("Test", "Speed", "km");

            var integratedSpeedSensor = new IntegrationSensor("Speed")
                {
                    Name = "IntegrationSensorSampleApp",
                    ID = "IntegrationSensorSampleAppId"
                };

            var averageSpeedSensor = new AverageSensor("Speed", 5000)
                {
                    Name = "AverageSensorSampleApp",
                    ID = "AverageSensorSampleAppId"
                };


            registry.Add(speedSensor);
            registry.Add(integratedSpeedSensor);
            registry.Add(averageSpeedSensor);

            registry.AddListener(speedSensor, OnSpeedChanged, 0);
            registry.AddListener(integratedSpeedSensor, OnIntegratedSpeedChanged, 0);
            registry.AddListener(averageSpeedSensor, OnAverageSpeedChanged, 0);

            for (var i = 0; i < 500; i++)
            {
                var sleepTime = 2000 + i * 10;
                speedSensor.Update(5 + i);
                _elapsedTime += sleepTime;
                Thread.Sleep(sleepTime);
            }
            /*
            for (var i = 0; i < 500; i++)
            {
                speedSensor.Update(10 + i);
                _elapsedTime += 2000;
                Thread.Sleep(2000);
            }
             */
            /*
            speedSensor.Update(10);
            Thread.Sleep(5000);
            elapsedTime += 5000;
            speedSensor.Update(20);
            Thread.Sleep(5000);
            elapsedTime += 5000;
            speedSensor.Update(30);
            Thread.Sleep(5000);
            elapsedTime += 5000;
            // Trig sensors
            speedSensor.Update(0);
            Thread.Sleep(5000);
            elapsedTime += 5000;
            */
            Console.WriteLine("Elapsed time = {0} ms", _elapsedTime);
            Console.WriteLine("Averge speed = {0}", averageSpeedSensor.Value);
            Console.WriteLine("Averge2 speed = {0}", integratedSpeedSensor.Value / integratedSpeedSensor.TotalTime);
            Console.ReadLine();
        }

        private static void OnSpeedChanged(Sensor s)
        {
            Console.WriteLine("SpeedSensor Value = {0} ", s.Value);
        }

        private static void OnIntegratedSpeedChanged(Sensor s)
        {
            Console.WriteLine("Average unlimited value = {0} ", s.Value / _elapsedTime);
        }

        private static void OnAverageSpeedChanged(Sensor s)
        {
            Console.WriteLine("Average limited Value = {0} ", s.Value);
        }
    }

}
