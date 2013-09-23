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
        public double Value{get{return getValue();} private set{}}
        //
        #endregion PROP

        #region FLD
        /// <summary>
        /// how many times the sensor value was updated
        /// </summary>
        private int updateCount = 0;
        /// <summary>
        /// base sensor whose value is accumulated
        /// </summary>
        private Sensor fuelConsumed;
        /// <summary>
        /// summary value
        /// </summary>
        private double sum;
        /// <summary>
        /// value read at each timeslot according to the slotcount
        /// </summary>
        private double[] timeslots;
        private double[] updateCountSlots;
        #endregion FLD

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseSensor"></param>
        /// <param name="interval"></param>
        /// <param name="slotsCount"></param>
        public IntegrationSensor(string baseSensor, int interval, int slotsCount = 50)
        {
            fuelConsumed = registry.Sensors.FirstOrDefault( s => s.Name == baseSensor && s.ID != this.ID);
            timeslots = new double[slotsCount];
            updateCountSlots = new double[slotsCount];

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
        }
        /// <summary>Suspend event indication for the sensor</summary>
        /// Means timebased calculation should be restarted
        /// because of temporary system unavailability.
        /// History could still be reused.
        public void Suspend()
        {
        }
        #endregion API

        #region UTL
        private double getValue()
        {
            if (Interval == 0)
            {
                sum = sum + fuelConsumed.Value;
                updateCount++;
                return sum;
            }
            else
            {
                return timeslots.Sum();
            }
        }
        #endregion UTL

    }
}
