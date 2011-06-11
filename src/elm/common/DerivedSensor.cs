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

        public override void Activate()
        {
            if (this.aid != null)
            {
                this.a = base.registry.Sensor(this.aid);
                base.registry.AddListener(this.a, this.OnSensorChange, Interval);
            }
            if (this.bid != null)
            {
                this.b = base.registry.Sensor(this.bid);
                base.registry.AddListener(this.b, this.OnSensorChange, Interval);
            }
        }

        public override void Deactivate()
        {
            base.registry.RemoveListener(this.OnSensorChange);
        }

        protected virtual void OnSensorChange(Sensor s)
        {
            this.Value = this.DerivedValue(this.a, this.b);
            base.TimeStamp = s.TimeStamp;
            base.registry.TriggerListeners(this);
        }
    }
}

