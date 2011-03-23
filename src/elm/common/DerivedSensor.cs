namespace hobd
{
    using System;

    public class DerivedSensor : CoreSensor
    {
        private Sensor a;
        protected string aid;
        private Sensor b;
        protected string bid;
        public Func<Sensor, Sensor, double> DerivedValue = (a, b) => 0.0;

        public DerivedSensor(string a, string b)
        {
            this.aid = a;
            this.bid = b;
        }

        public override void Activate()
        {
            if (this.aid != null)
            {
                this.a = base.registry.Sensor(this.aid);
                base.registry.AddListener(this.a, new Action<Sensor>(this.OnSensorChange));
            }
            if (this.bid != null)
            {
                this.b = base.registry.Sensor(this.bid);
                base.registry.AddListener(this.b, new Action<Sensor>(this.OnSensorChange));
            }
        }

        public override void Deactivate()
        {
            base.registry.RemoveListener(new Action<Sensor>(this.OnSensorChange));
        }

        protected virtual void OnSensorChange(Sensor s)
        {
            this.Value = this.DerivedValue(this.a, this.b);
            base.TimeStamp = s.TimeStamp;
            base.registry.TriggerListeners(this);
        }
    }
}

