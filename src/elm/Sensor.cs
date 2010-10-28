using System;
using System.Collections.Generic;

namespace hobd
{

/**
 * Sensor interface.
 * 
 */
public interface Sensor
{
    double Value{get;}

    DateTime TimeStamp{get;}
    
    string ID{get;}

    string Name{get;}
    
    string Description{get;}
    
    string GetDescription(string lang);
    
    string Units{get;}
    
    IEnumerable<string> Aliases{get;}
    
    /** Sensor itself may depend on other sensors,
     *    or it may read vehicle parameters from registry.
     */
    void SetRegistry(SensorRegistry registry);
    
    void NotifyAddListener(Action<Sensor> listener);
    
    void NotifyRemoveListener(Action<Sensor> listener);
}
    
}
