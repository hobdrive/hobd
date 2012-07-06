namespace hobd
{
    using System;

    public class DerivedSensor : CoreSensor
    {
        private Sensor a;
        protected string aid;
        private Sensor b;
        protected string bid;
        protected bool triggerA, triggerB;
        protected int Interval;
        public Func<Sensor, Sensor, double> DerivedValue = (a, b) => 0.0;

        public DerivedSensor(string id, string a, string b) : this(id, a,b,0)
        {
        }

        public DerivedSensor(string id, string a, string b, int interval) : this(id, a, b, true, true, interval)
        {
        }

        public DerivedSensor(string id, string a, string b, bool triggerA, bool triggerB) : this(id, a,b, triggerA, triggerB, 0)
        {
        }
        public DerivedSensor(string id, string a, string b, bool triggerA, bool triggerB, int interval)
        {
            this.aid = a;
            this.bid = b;
            this.triggerA = triggerA;
            this.triggerB = triggerB;
            this.Interval = interval;
            this.ID = this.Name = id;
        }

        public virtual void LoadBaseSensors()
        {
            if (this.aid != null && this.a == null)
                this.a = base.registry.Sensor(this.aid);
            if (this.bid != null && this.b == null)
                this.b = base.registry.Sensor(this.bid);
        }

        public override void Activate()
        {
            Logger.dump("DerivedSensor", "Activate: "+this.ID);
            LoadBaseSensors();
            if (this.a != null)
                base.registry.AddListener(this.a, this.OnSensorChange, Interval);
            if (this.b != null)
                base.registry.AddListener(this.b, this.OnSensorChange, Interval);
        }

        public override void Deactivate()
        {
            base.registry.RemoveListener(this.OnSensorChange);
            this.a = this.b = null;
        }
    
        protected bool recurseValue = false;
        public override double Value {
            get{                    
                if (recurseValue || (this.Active && this.Valid)){
                    return this.value;
                }else{
                    // First time init
                    try{
                        LoadBaseSensors();
                        recurseValue = true;
                        this.Value = this.DerivedValue(this.a, this.b);
                        recurseValue = false;
                        base.TimeStamp = DateTimeMs.Now;
                    }catch(Exception){
                        // ignore it - there could be problems on init stage
                    }
                    return this.value;
                }
            }
            protected set{
                this.value = value;
                base.Valid = true;
            }
        }

        protected virtual void OnSensorChange(Sensor s)
        {
            this.Value = this.DerivedValue(this.a, this.b);
            base.TimeStamp = s.TimeStamp;
            if ((triggerA && s == this.a) || (triggerB && s == this.b))
                base.registry.TriggerListeners(this);
        }
    }
}

