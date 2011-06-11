namespace hobd
{
    using System;

    public interface IFuelCostSensor
    {
        string FuelCurrency { get; set; }

        double FuelPrice { get; set; }

        string FuelPriceString { get; set; }
    }
}

