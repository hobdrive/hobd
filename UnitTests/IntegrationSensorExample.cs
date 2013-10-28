using System;
using System.Threading;
using hobd;
using NUnit.Framework;
using hobd.src.elm.common;

namespace IntegrationSensorTest
{
    internal class IntegrationSensorExample
    {
        private static void Main(string[] args)
        {
            const int timeMs = 1000;
            var registry = new SensorRegistry();

            var speedSensor = new CoreSensorEx("Test", "Speed", "km");

            var integratedSpeedSensor = new IntegrationSensor("Speed")
             {
               ID = "IntegrationSensor"
            };
            
            var averageSpeedSensor = new AverageSensor("Speed")
                {
                    ID = "AverageSensorSampleApp"
                };
            

            registry.Add(speedSensor);
            registry.Add(integratedSpeedSensor);
            registry.Add(averageSpeedSensor);

            registry.AddListener(speedSensor, OnSpeedChanged, 0);
            registry.AddListener(integratedSpeedSensor, OnIntegratedSpeedChanged, 0);
            registry.AddListener(averageSpeedSensor, OnAverageSpeedChanged, 0);

            int testCount = 20;
            long elapsedTime = 0;
            /*
            for (var i = 0; i < testCount; i++)
            {
                speedSensor.Update(10 + i);                
                int sleepTime = timeMs + i*50;
                Thread.Sleep(sleepTime);                
                elapsedTime += sleepTime;                
                Console.WriteLine("Time interval= {0} ms", sleepTime);
            }
            */
            speedSensor.Update(10);
            Thread.Sleep(5000);
            elapsedTime += 5000;
            speedSensor.Update(20);
            Thread.Sleep(5000);
            elapsedTime += 5000;
            speedSensor.Update(40);
            Thread.Sleep(5000);
            elapsedTime += 5000;
            speedSensor.Update(10);
            Thread.Sleep(2000);
            elapsedTime += 2000;
            
            Console.WriteLine("Elapsed time = {0} ms", elapsedTime);
            Console.WriteLine("Averge speed = {0}", averageSpeedSensor.Value);
            Console.ReadLine();
        }

        private static void OnSpeedChanged(Sensor s)
        {
            Console.WriteLine("SpeedSensor Value = {0} ", s.Value);
        }

        private static void OnIntegratedSpeedChanged(Sensor s)
        {
            Console.WriteLine("IntegratedSpeedSensor Value = {0} ", s.Value);
        }

        private static void OnAverageSpeedChanged(Sensor s)
        {
            Console.WriteLine("AverageSpeedSensor Value = {0} ", s.Value);
        }
    }

}
