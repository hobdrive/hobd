using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hobd
{
    internal struct SensorData
    {
        public double Value { get; set; }
        public long TimeStamp { get; set; }
    }

    internal class CircularBuffer
    {
        private SensorData[] _buffer;
        private int _bufferPtr;
        private object _syncObject;
        private int _bufferSize;

        public CircularBuffer(int bufferSize)
        {
            InitializeBuffer(bufferSize);
        }

        public void Add(SensorData sensorData)
        {
            lock (_syncObject)
            {
                _buffer[_bufferPtr] = sensorData;
                ShiftPtr(ref _bufferPtr);
            }
        }

        public SensorData[] Get()
        {
            var resBuf= new SensorData[_bufferSize];
            lock (_syncObject)
            {
                int count = (_bufferSize < _buffer.Count()) ? _bufferSize : _buffer.Count();
                int shift = _bufferPtr;

                for (int i = 0; i < count; i++)
                {
                    resBuf[i]=_buffer[shift];
                    ShiftPtr(ref shift);
                }
            }
            return resBuf.ToArray();
        }

        private void InitializeBuffer(int bufferSize)
        {
            if (bufferSize < 1)
            {
                var msg = String.Format("BufferSize must be positive. Current size is {0}", bufferSize);
                throw new ArgumentOutOfRangeException(msg);
            }

            _buffer = new SensorData[bufferSize];
            _bufferSize = bufferSize;
            _bufferPtr = 0;
            _syncObject = new object();
        }

        private void ShiftPtr(ref int ptr)
        {
            ptr++;
            if (ptr >= _bufferSize)
            {
                ptr = 0;
            }

        }

    }

    public class IntegrationSensor : CoreSensor, IAccumulatorSensor
    {
        readonly object _syncObject = new object();
        //
        //CircularBuffer _sensorDataBuffer;
        public const int DEFAULT_SLOTS_COUNT = 50;
        //
        Sensor _baseSensor;
        readonly string _baseSensorId;
        //
        bool _firstRun = true;
        bool _suspendCalculations;
        long _previouseTimeStamp;
        long _totalTime;
        double _sum;
        //
        readonly int _interval;
        readonly int _slotsCount;
        // 
        double[] _valuesBuffer;
        long[] _timestampsBuffer;
        //
        int _valuesBufferIndex = 0;
        int _timestampsBufferIndex = 0;

        #region Constructors

        public IntegrationSensor(string baseSensorId)
            : this(baseSensorId, 0, DEFAULT_SLOTS_COUNT)
        {

        }

        public IntegrationSensor(string baseSensorId, int interval)
            : this(baseSensorId, interval, DEFAULT_SLOTS_COUNT)
        {
            
        }

        public IntegrationSensor(string baseSensorId, int interval, int slotsCount)
        {
            _baseSensorId = baseSensorId;
            _interval = interval;
            _slotsCount = slotsCount;

            if (interval > 0)
            {
                //_sensorDataBuffer = _slotsCount >= DEFAULT_SLOTS_COUNT ? new CircularBuffer(slotsCount) : new CircularBuffer(DEFAULT_SLOTS_COUNT);
                _timestampsBuffer = _slotsCount >= DEFAULT_SLOTS_COUNT ? new long[slotsCount] : new long[DEFAULT_SLOTS_COUNT];
                _valuesBuffer = _slotsCount >= DEFAULT_SLOTS_COUNT ? new double[slotsCount] : new double[DEFAULT_SLOTS_COUNT];
                
            }
        }

        #endregion;

        public void Reset()
        {
            this.value = 0; 
            _sum = 0; 
            _totalTime = 0;
            _firstRun = true;
            _previouseTimeStamp = 0;
            this.TimeStamp = 0;
            //
            if (_interval > 0)
            {
                //_sensorDataBuffer = new CircularBuffer(_slotsCount);
                _valuesBuffer = new double[_interval / 1000];
                _timestampsBuffer = new long[_interval / 1000];
            }
        }

        public void Suspend()
        {
            _suspendCalculations = true;
        }

        public override double Value
        {
            get
            {
                lock (_syncObject)
                {
                    return this.value;
                }
                // There is no need to update value relatively current time, just return value
                // move this calculations into Limited and Unlimited calculations logic
                #region unused code
                /*
                if (_interval > 0)
                {
                    double avgSpeeds = 0;
                    long avgTimeIntervals = 0;
                    double valueLocal;
                    long previouseTimeLocal;

                    lock (_syncObject)
                    {
                        valueLocal = _value;
                        previouseTimeLocal = _previouseTime;
                    }
                    

                    var bufferedData = _sensorDataBuffer.Get();
                    var currentTime = DateTimeMs.Now;
                    var satisfiedTime = currentTime - _interval;
                    for (var i = 0 ; i < bufferedData.Count() -1 ; i++)
                    {
                        var sensorData0 = bufferedData[i];
                        var sensorData1 = bufferedData[i + 1];

                        if (sensorData1.TimeStamp <= satisfiedTime)
                            continue;
                        var t0 = sensorData0.TimeStamp >= satisfiedTime
                                      ? sensorData0.TimeStamp
                                      : satisfiedTime;
                        var t1 = sensorData1.TimeStamp;

                        avgSpeeds += sensorData0.Value*(t1 - t0);
                        avgTimeIntervals += (t1 - t0);
                    }

                    #if DEBUG_LIMITED

                    var deltaTime = currentTime - previouseTimeLocal;
                    double tmpValue = valueLocal * deltaTime;
                    avgSpeeds += tmpValue;
                    long tmpTime = avgTimeIntervals + deltaTime;
                    double v = avgSpeeds / tmpTime;
                    return v;

                    #else

                    return (avgSpeeds + (valueLocal * (currentTime - previouseTimeLocal))) / (avgTimeIntervals + (currentTime - previouseTimeLocal));    

                    #endif
                }
                else
                {
                    lock (_syncObject)
                    {
                        var currentTime = DateTimeMs.Now;
                        return (_sum + (_value * (currentTime - _previouseTime))) / (_avgTime + (currentTime - _previouseTime));
                    }
                }
                 */
                #endregion
            }
        }

        public long TotalTime
        {
            get
            {
                // There is no need to update value regarding current time just return value
                lock (_syncObject)
                {
                    return _totalTime;
                }
            }
        }

        protected bool FirstRun
        {
            get
            {
                return _firstRun || (this.TimeStamp - _previouseTimeStamp) < 0 ||
                       (this.TimeStamp - _previouseTimeStamp) > _interval;
            }
            set { _firstRun = value; }

        }

        protected override void Activate()
        {
            Reset();
            try
            {
                _baseSensor = registry.Sensor(_baseSensorId, this);
            }
            catch (Exception)
            {
                throw new NullReferenceException("Null sensor");
            }
            registry.AddListener(_baseSensor, OnBasedSensorChange);
        }

        protected override void Deactivate()
        {
            registry.RemoveListener(OnBasedSensorChange);
        }

        void OnBasedSensorChange(Sensor s)
        {
            if (_interval > 0)
            {
                TimeLimitedCalculation(s);
            }
            else
            {
                TimeUnLimitedCalculation(s);
            }
            registry.TriggerListeners(this);
        }

        private void TimeLimitedCalculation(Sensor s)
        {
            if (FirstRun)
            {
                _previouseTimeStamp = s.TimeStamp;
                this.TimeStamp = s.TimeStamp;
                _firstRun = false;
                _suspendCalculations = false;
                return;
            }

            #region this version is similar to FuelEconomyPeriodSensor

            if (!_suspendCalculations)
            {
                this.TimeStamp = s.TimeStamp;
                //
                var index = (int)((TimeStamp / 1000) % _valuesBuffer.Length);
                //
                if (index != _valuesBufferIndex)
                {
                    _sum -= _valuesBuffer[index];
                    _valuesBuffer[index] = 0;
                    _valuesBufferIndex = index;
                }
                var currentValue = s.Value * ((TimeStamp - _previouseTimeStamp)) / 1000;
                _valuesBuffer[_valuesBufferIndex] += currentValue;
                //
                if (index != _timestampsBufferIndex)
                {
                    _totalTime -= _timestampsBuffer[index];
                    _timestampsBuffer[index] = 0;
                    _timestampsBufferIndex = index;
                }
                var timeInterval = ((TimeStamp - _previouseTimeStamp)) / 1000;
                _timestampsBuffer[_timestampsBufferIndex] += timeInterval;
                //
                lock (_syncObject)
                {
                    _sum += currentValue;
                    this.value = _sum;
                    _totalTime += timeInterval;    
                }
            }

            #endregion
            
            #region this version calculates total time and total value for the passed interval using CircularBuffer
            /*
            double totalValue = 0;
            long totalTimeIntervals = 0;
            //
            if (!_suspendCalculations)
            {
                _sensorDataBuffer.Add(new SensorData { Value = s.Value, TimeStamp = s.TimeStamp });
                
                //
                var bufferedData = _sensorDataBuffer.Get();
                //
                var currentTime = s.TimeStamp;
                var satisfiedTime = currentTime - _interval;
                var prevTimeStamp = satisfiedTime;
                //
                for (var i = 0; i < bufferedData.Count() - 1; i++)
                {
                    if (bufferedData[i].TimeStamp < satisfiedTime)
                        continue;
                    var t0 = prevTimeStamp;
                    var t1 = bufferedData[i].TimeStamp;

                    totalValue += bufferedData[i].Value * (t1 - t0);
                    totalTimeIntervals += (t1 - t0);

                    prevTimeStamp = bufferedData[i].TimeStamp;
                }
                lock (_syncObject)
                {
                    this.value = totalValue;
                    _totalTime = totalTimeIntervals;
                    this.TimeStamp = s.TimeStamp;
                }
            }
             */
            #endregion
            
            _previouseTimeStamp = s.TimeStamp;

        }

        private void TimeUnLimitedCalculation(Sensor s)
        {
            if (FirstRun)
            {
                _previouseTimeStamp = s.TimeStamp; 
                this.TimeStamp = s.TimeStamp;
                _firstRun = false;
                _suspendCalculations = false;
                return;
            }
            //
            if (!_suspendCalculations)
            {
                lock (_syncObject)
                {
                    _sum += s.Value * (s.TimeStamp - _previouseTimeStamp);
                    _totalTime += s.TimeStamp - _previouseTimeStamp;
                    this.TimeStamp = s.TimeStamp;
                    this.value = _sum;
                }
            }
            _previouseTimeStamp = s.TimeStamp;
        }
    }
}
