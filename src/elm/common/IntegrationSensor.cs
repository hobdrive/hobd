using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hobd
{
    public class IntegrationSensor : CoreSensor, IAccumulatorSensor
    {
        #region PROP
        //
        /// <summary>
        /// Sum interval in milliseconds
        /// </summary>
        public int Interval{get;set;}
        /// <summary>
        /// 
        /// </summary>
        public int SlotsCount{get;set;}
        /// <summary>
        /// 
        /// </summary>
        public double Value { get; private set; }
        //
        #endregion PROP

        #region CLS
        #endregion CLS

        #region FLD
        /// <summary>
        /// how many times the sensor value was updated
        /// </summary>
        private int updateCount = 0;
        /// <summary>
        /// base sensor whose value is accumulated
        /// </summary>
        private Sensor BaseSensor;
        private string BaseSensorName;
        /// <summary>
        /// summary value
        /// </summary>
        private double sum;
        /// <summary>
        /// value read at each timeslot according to the slotcount
        /// </summary>
        private double[] timeslots;
        #endregion FLD

        /// <summary>
        /// 60 Sec. default for interval
        /// </summary>
        /// <param name="baseSensorName"></param>
        public IntegrationSensor(string baseSensorName):
            this(baseSensorName, 60)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseSensor">sensor we are accumlating</param>
        /// <param name="interval">interval on which to aggregate base sensor readings</param>
        /// <param name="slotsCount"></param>
        public IntegrationSensor(string baseSensorName, int interval, int slotsCount = 50)
        {
            SlotsCount = slotsCount;
            timeslots = new double[SlotsCount];
            BaseSensorName = baseSensorName;
            this.Interval = interval;
        }

        #region API
        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            this.value = 0;
            this.updateCount = 0;
            timeslots = new double[SlotsCount];

            //Interval & BaseSensor remain the same values as from the constructor
        }
        /// <summary>Suspend event indication for the sensor</summary>
        /// Means timebased calculation should be restarted
        /// because of temporary system unavailability.
        /// History could still be reused.
        public void Suspend()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="registry"></param>
        public override void SetRegistry(SensorRegistry registry)
        {
            base.SetRegistry(registry);
            BaseSensor = registry.Sensors.FirstOrDefault(s => s.Name == BaseSensorName && s.ID != this.ID);
        }
        protected override void Activate()
        {
            registry.AddListener(BaseSensor, OnSensorChange, Interval);
        }
        protected override void Deactivate()
        {
            registry.RemoveListener(OnSensorChange);
        }
        #endregion API

        #region EVT
        public void OnSensorChange(Sensor s)
        {
            TimeStamp = s.TimeStamp;

            Value = ComputeNewValue();
            timeslots[updateCount] = Value;
            registry.TriggerListeners(this);
        }
        #endregion EVT

        #region UTL
        private double ComputeNewValue()
        {
            //Unlimited sum
            if (Interval == 0)
            {
                sum = sum + BaseSensor.Value;
                updateCount++;
                return sum;
            }
            else //Limited sum
            {
                return timeslots.Sum();
            }
        }
        #endregion UTL

    }
}
