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
    
    /** Unique ID for the sensor */
    string ID{get;}

    /** Public system name **/
    string Name{get;}
    
    string Units{get;}

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
