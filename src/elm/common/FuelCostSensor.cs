namespace hobd
{
    using System;
    using System.Runtime.CompilerServices;

    public class FuelCostSensor : DerivedSensor, IFuelCostSensor
    {
        public FuelCostSensor(string fuelConsumedSensor) : base(fuelConsumedSensor, null)
        {
            Func<Sensor, Sensor, double> func = null;
            if (func == null)
            {
                func = (consumed, b) => consumed.Value * this.FuelPrice;
            }
            base.DerivedValue = func;
            this.ID = fuelConsumedSensor + "_Cost";
            this.Name = this.ID;
        }

        public string FuelCurrency
        {
            get
            {
                return this.Units;
            }
            set
            {
                this.Units = value;
            }
        }

        public double FuelPrice { get; set; }

        public string FuelPriceString
        {
            get
            {
                return this.FuelPrice.ToString();
            }
            set
            {
                try
                {
                    this.FuelPrice = double.Parse(value, UnitsConverter.DefaultNumberFormat);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}

