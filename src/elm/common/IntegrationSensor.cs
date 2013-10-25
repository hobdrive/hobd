using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hobd
{
    internal struct SensorData
    {
        public double Value { get; set; }
        
        public long TimeStampStart { get; set; }
        public long TimeStampEnd { get; set; }
    }

    internal class CircularBuffer
    {
        private List<SensorData> _buffer;
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
                if (_buffer.Count < _bufferSize)
                {
                    _buffer.Add(sensorData);
                }
                else
                {
                    _buffer[_bufferPtr] = sensorData;
                    ShiftPtr(ref _bufferPtr);
                }
            }
        }

        public List<SensorData> Get()
        {
            var resBuf= new List<SensorData>();

            lock (_syncObject)
            {
                int count = (_bufferSize < _buffer.Count) ? _bufferSize : _buffer.Count;
                int shift = _bufferPtr;

                for (int i = 0; i < count; i++)
                {
                    resBuf.Add(_buffer[shift]);
                    ShiftPtr(ref shift);
                }
            }
            return resBuf;
        }

        private void InitializeBuffer(int bufferSize)
        {
            if (bufferSize < 1)
            {
                var msg = String.Format("BufferSize must be positive. Current size is {0}", bufferSize);
                throw new ArgumentOutOfRangeException(msg);
            }

            _buffer = new List<SensorData>();
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
        private CircularBuffer _sensorDataBuffer;
        const int DEFAULT_SLOTS_COUNT = 50;
        //
        Sensor baseSensor;
        readonly string BaseSensorId;
        //
        bool FirstRun = true;
        bool SuspendCalculations = false;
        long _previouseTime;
        long _avgTime;
        double _sum;
        //
        double _value;
        int _interval;
        int _slotsCount;


        public IntegrationSensor(string baseSensor_Id)
            : this(baseSensor_Id, 0, DEFAULT_SLOTS_COUNT)
        {

        }

        public IntegrationSensor(string baseSensor_Id, int interval)
            : this(baseSensor_Id, interval, DEFAULT_SLOTS_COUNT)
        {
            
        }

        public IntegrationSensor(string baseSensor_Id, int interval, int slotsCount)
        {
            BaseSensorId = baseSensor_Id;
            _interval = interval;
            _slotsCount = slotsCount;

            if (interval > 0)
            {
                _sensorDataBuffer = _slotsCount >= DEFAULT_SLOTS_COUNT ? new CircularBuffer(slotsCount) : new CircularBuffer(DEFAULT_SLOTS_COUNT);
                
            }
        }

        public void Reset()
        {
            _value = 0;
            FirstRun = true;
            if (Interval > 0)
            {
                _sensorDataBuffer = new CircularBuffer(SlotsCount);
            }
        }

        public void Suspend()
        {
            FirstRun = true;
        }

        public int Interval 
        {
            get { return this._interval; }
            set { this._interval = value; }
        }

        public int SlotsCount
        {
            get { return this._slotsCount; }
            set { this._slotsCount = value; }

        }

        public SensorRegistry Registry
        {
            get
            {
                if (registry == null)
                {
                    throw new NullReferenceException("Null registry");
                }
                return registry;
            }
        }

        public Sensor BaseSensor
        {
            get
            {
                if (baseSensor == null)
                {
                    try
                    {
                        baseSensor = Registry.Sensor(BaseSensorId, this);
                    }
                    catch (Exception)
                    {
                        throw new NullReferenceException("Null sensor");
                    }
                    
                }
                return baseSensor;
            }
            set { 
                baseSensor = value;
            }
        }
        
        public override double Value
        {
            get
            {
                double avgSpeeds = 0;
                long avgTimeIntervals = 0;
                if (Interval > 0)
                {
                    var bufferedData = _sensorDataBuffer.Get();
                    var currentTime = DateTimeMs.Now;
                    var satisfiedTime = currentTime - Interval;
                    foreach (var sensorData in bufferedData)
                    {
                        if (sensorData.TimeStampEnd < satisfiedTime)
                            continue;
                        var t0 = sensorData.TimeStampStart > satisfiedTime
                                      ? sensorData.TimeStampEnd - sensorData.TimeStampStart
                                      : satisfiedTime;
                        var t1 = sensorData.TimeStampEnd;

                        avgSpeeds += sensorData.Value*(t1 - t0);
                        avgTimeIntervals += (t1 - t0);
                    }

                    _sum = avgSpeeds/avgTimeIntervals;
                    lock (_syncObject)
                    {
                        return (_sum + (_value * (DateTimeMs.Now - _previouseTime))) / (_avgTime + (DateTimeMs.Now - _previouseTime));    
                    }
                }
                else
                {
                    lock (_syncObject)
                    {
                        return (_sum + (_value * (DateTimeMs.Now - _previouseTime))) / (_avgTime + (DateTimeMs.Now - _previouseTime));
                    }    
                }
            }
        }

        protected override void Activate()
        {
            Reset();
            Registry.AddListener(BaseSensor, OnBasedSensorChange);
        }

        protected override void Deactivate()
        {
            Reset();
            Registry.RemoveListener(OnBasedSensorChange);
        }

        void OnBasedSensorChange(Sensor s)
        {
            if (Interval > 0)
            {
                TimeLimitedCalculation(s);
            }
            else
            {
                TimeUnLimitedCalculation(s);
            }
        }

        private void TimeLimitedCalculation(Sensor s)
        {
            if (FirstRun)
            {
                _value = s.Value;
                _previouseTime = s.TimeStamp;
                SuspendCalculations = false;
                return;
            }
            if (!SuspendCalculations)
            {
                lock (_syncObject)
                {
                    _sensorDataBuffer.Add(new SensorData() { Value = _value,TimeStampStart = _previouseTime, TimeStampEnd = s.TimeStamp });
                    _previouseTime = s.TimeStamp;
                    _value = s.Value;
                }
            }
            registry.TriggerListeners(this);
            
        }

        private void TimeUnLimitedCalculation(Sensor s)
        {
            if (FirstRun)
            {
                _value = s.Value;
                _previouseTime = s.TimeStamp;
                FirstRun = false;
                SuspendCalculations = false;
                return;
            }
            //
            if (!SuspendCalculations)
            {
                lock (_syncObject)
                {
                    _sum += _value * (s.TimeStamp - _previouseTime);
                    _avgTime += s.TimeStamp - _previouseTime;
                    _previouseTime = s.TimeStamp;
                    _value = s.Value;
                }
            }
            registry.TriggerListeners(this);
        }
    }
}
