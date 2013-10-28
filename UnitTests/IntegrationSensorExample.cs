using System;
using System.Threading;
using hobd;
using NUnit.Framework;

namespace IntegrationSensorTest
{
    internal class IntegrationSensorExample
    {
        private static void Main(string[] args)
        {
            const int timeMs = 1000;
            var registry = new SensorRegistry();

            var speedSensor = new CoreSensorEx("Test", "Speed", "km");
            var integratedSpeedSensor = new IntegrationSensor("Speed"/*,60000*/)
                {
                    ID = "IntegrationSensor"
                };

            registry.Add(speedSensor);
            registry.Add(integratedSpeedSensor);

            registry.AddListener(speedSensor, OnSpeedChanged, 0);
            registry.AddListener(integratedSpeedSensor, OnIntegratedSpeedChanged, 0);

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
            Thread.Sleep(29000);
            speedSensor.Update(20);
            Thread.Sleep(29000);
            //var v = integratedSpeedSensor.Value;
            Console.WriteLine("Elapsed time = {0} ms", elapsedTime);
            Console.WriteLine("Averge speed = {0}", integratedSpeedSensor.Value);
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
    }

}
