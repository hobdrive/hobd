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
        /*
        private struct Readings
        {
            public static double Displacement { get; set; }
        }
         */
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
        /// 
        /// </summary>
        /// <param name="baseSensor">sensor we are accumlating</param>
        /// <param name="interval">interval on which to aggregate base sensor readings</param>
        /// <param name="slotsCount"></param>
        public IntegrationSensor(string baseSensorName, int interval, int slotsCount = 50)
        {
            SlotsCount = slotsCount;
            BaseSensor = registry.Sensors.FirstOrDefault(s => s.Name == baseSensorName && s.ID != this.ID);
            timeslots = new double[SlotsCount];

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

            /*
             * //NOT SURE WHETHERE I NEED TO REFER TO BASE SENSORS BY NAME
            try
            {
                Readings.Displacement = double.Parse(registry.VehicleParameters["liters"], UnitsConverter.DefaultNumberFormat);
            }
            catch (Exception)
            {
                Logger.info("LitersPerHourSensor", "Using default displacement and VE ratio");
            }
             */

        }
        protected override void Activate()
        {
            /*
            map = registry.Sensor(OBD2Sensors.IntakeManifoldPressure);
            registry.AddListener(map, OnSensorChange, ListenInterval);

            rpm = registry.Sensor(OBD2Sensors.RPM);
            registry.AddListener(rpm, OnSensorChange, ListenInterval);

            iat = registry.Sensor(OBD2Sensors.IntakeAirTemp);
            registry.AddListener(iat, OnSensorChange, 3000 + ListenInterval);
             */


            //not sure if i should invoke OBD2Sensors or they are encapsulated in hobd.Sensor?
            registry.AddListener(BaseSensor, OnSensorChange);
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

            Value = getValue();
            timeslots[updateCount] = Value;
            registry.TriggerListeners(this);
        }
        #endregion EVT

        #region UTL
        private double getValue()
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
