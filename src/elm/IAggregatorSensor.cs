using System;
using System.Collections.Generic;

namespace hobd
{

/// <summary>Aggregator sensor always takes some history of a sensor data to provide its current value</summary>
public interface IAggregatorSensor
{
    /// <summary>Reset action for the sensor</summary>
    /// Means total reset just happened, accumulated data should be resetted.
    /// All calculations should be restarted from scratch.
    void Reset();
    
    /// <summary>Suspend event indication for the sensor</summary>
    /// Means timebased calculation should be restarted
    /// because of temporary system unavailability.
    /// History could still be reused.
    void Suspend();
}

}
