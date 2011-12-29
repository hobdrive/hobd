namespace hobd
{
    using System;

    public class DerivedSensor : CoreSensor
    {
        private Sensor a;
        protected string aid;
        private Sensor b;
        protected string bid;
        protected int Interval;
        public Func<Sensor, Sensor, double> DerivedValue = (a, b) => 0.0;

        public DerivedSensor(string id, string a, string b) : this(id, a,b,0)
        {
        }

        public DerivedSensor(string id, string a, string b, int interval)
        {
            this.aid = a;
            this.bid = b;
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
            LoadBaseSensors();
            if (this.a != null)
                base.registry.AddListener(this.a, this.OnSensorChange, Interval);
            if (this.b != null)
                base.registry.AddListener(this.b, this.OnSensorChange, Interval);
        }

        public override void Deactivate()
        {
            base.registry.RemoveListener(this.OnSensorChange);
        }
    
        double value;
        public override double Value {
            get{
                if (this.Active){
                    return this.value;
                }else{
                    LoadBaseSensors();
                    this.value = this.DerivedValue(this.a, this.b);
                    return this.value;
                }
            }
            protected set{
                this.value = value;
            }
        }

        protected virtual void OnSensorChange(Sensor s)
        {
            this.Value = this.DerivedValue(this.a, this.b);
            base.TimeStamp = s.TimeStamp;
            base.registry.TriggerListeners(this);
        }
    }
}

