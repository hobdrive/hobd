namespace hobd
{
    using System;

    /// <summary>
    /// Interface for sensors, who are related to global current fuel price value
    /// </summary>
    public interface IFuelCostSensor
    {
        /// <summary>
        /// Fuel current currency, used in the system
        /// </summary>
        string FuelCurrency { get; set; }

        /// <summary>
        /// Fuel current price (in current currency), used in the system
        /// </summary>
        double FuelPrice { get; set; }

        /// <summary>
        /// String representation of fuel price 
        /// </summary>
        string FuelPriceString { get; set; }
    }
}

