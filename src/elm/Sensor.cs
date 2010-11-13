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
    /** Sensor current value. always double. */
    double Value{get;}

    /** Sensor current value's timestamp. in ms */
    long TimeStamp{get;}
    
    /** ID for the sensor */
    string ID{get;}

    string Name{get;}
    string GetName(string lang);
    
    string Description{get;}
    string GetDescription(string lang);
    
    string Units{get;}
    string GetUnits(string lang);
 
    /** Possible other IDs to refer this sensor */
    IEnumerable<string> Aliases{get;}
    
    /**
     *  Sensor itself may depend on other sensors,
     *  or it may read vehicle parameters from registry.
     */
    void SetRegistry(SensorRegistry registry);
    /**
     * It is possible to dynamically deregister sensor
     */
    void DetachRegistry();
    
    void NotifyAddListener(Action<Sensor> listener);
    
    void NotifyRemoveListener(Action<Sensor> listener);
}
    
}
