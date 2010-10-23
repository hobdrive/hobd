using System;
using System.Collections.Generic;

namespace hobd
{

public interface IAccumulatorSensor
{
    /**
     * Reset action for the sensor. Means total reset just happened, accumulated data should be resetted.
     */
    void Reset();
    
    /**
     * Suspend action for the sensor. Means timebased calculation should be restarted
     * because of temporary system unavailability.
     */
    void Suspend();
}

}
