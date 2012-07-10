using System;
using System.Collections.Generic;

namespace hobd
{

/// <summary>Sensor interface</summary>
/// <para>This is a common interface for all sensors</para>
/// Each sensor provides its Value and TimeStamp (in ms)
public interface Sensor
{
    /// <summary>Sensor current value. always double.</summary>
    double Value{get;}

    /// <summary>Validness state of a sensor</summary>
    /// Whether the sensor is active and its value is correctly read
    /// false means sensor can't read its value (or its value is not yet ready) right now
    bool Valid{get;}

    /// <summary>Availability state of a sensor</summary>
    /// Whever the sensor is available for current setup
    bool Available{get;}

    /// <summary>Sensor's last update timestamp</summary>
    /// Sensor current value's timestamp. in ms
    long TimeStamp{get;}
    
    /// 
    /// <summary>Unique ID for the sensor</summary>
    /// ID should be unique, namespace.name notation is normally used for this.
    /// Namespace gives a domain where sensor is defined, name gives a meaningful name
    string ID{get;}

    /// <summary>Public system name of the sensor</summary>
    /// Name should be as simple as possible.
    /// It is normal for name not to be unique, since sensors from different domains (with different implementation) may expose the same meaning.
    /// F.e. there could be multiple <c>liters per hour</c> sensors:
    /// MAF.LitersPerHour
    /// MAP.LitersPerHour
    /// Injector.LitersPerHour
    string Name{get;}
    
    /// <summary>Name of the measurement units of this sensor</summary>
    string Units{get;}

    /// <summary>Informs sensor about sensor registry attachment</summary>
    /// Sensor itself may depend on other sensors,
    /// or it may read vehicle parameters from registry.
    /// This method informs the sensor that it was attached to a group of sensors, under management of a
    /// SensorRegistry. 
    void SetRegistry(SensorRegistry registry);

    /// <summary>Informs the sensor that it should be detached from sensor registry</summary>
    /// It therefore should not take any more evaluations
    void DetachRegistry();
    
	/// <summary>Sensor is informed by this method that in SensorRegistry appeared an entry for this sensor listener</summary>
	/// Sensor should activate itself as soon as first listener is added
    void NotifyAddListener(Action<Sensor> listener);
    
	/// <summary>Sensor is informed by this method that SensorRegistry removed a listener for this sensor</summary>
	/// Sensor should deactivate itself and stop all activities as soon as last listener is removed
    void NotifyRemoveListener(Action<Sensor> listener);
}
    
}
