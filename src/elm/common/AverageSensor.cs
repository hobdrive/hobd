using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hobd.src.elm.common
{
    public class AverageSensor : CoreSensor
    {
        private readonly object _syncObject = new object();
        private readonly IntegrationSensor _integratedSensor;

        public AverageSensor(string baseSensorId)
            : this(baseSensorId, 0, IntegrationSensor.DEFAULT_SLOTS_COUNT)
        {
            
        }

        public AverageSensor(string baseSensorId, int interval)
            :this(baseSensorId, interval, IntegrationSensor.DEFAULT_SLOTS_COUNT)
        {
            
        }

        public AverageSensor(string baseSensorId, int interval, int slotsCount)
        {
            _integratedSensor = new IntegrationSensor(baseSensorId, interval, slotsCount)
            // How to set correctly Id and Name ???????????
                {
                    Name = "IntegrationSensor",
                    ID = "IntegrationSensor" + "Id"
                };
        }

        public override double Value
        {
            get
            {
                lock (_syncObject)
                {
                    return base.value;   
                }
            }
        }

        protected override void Activate()
        {
            registry.Add(_integratedSensor);
            registry.AddListener(_integratedSensor, OnIntegratedSensorChange);
        }

        protected override void Deactivate()
        {
            registry.RemoveListener(OnIntegratedSensorChange);
        }

        void OnIntegratedSensorChange(Sensor s)
        {
            lock (_syncObject)
            {
                base.value = _integratedSensor.Value / _integratedSensor.TotalTime;    
            }
            registry.TriggerListeners(this);
        }
    }
}
