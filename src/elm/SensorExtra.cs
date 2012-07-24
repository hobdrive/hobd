using System;
using System.Collections.Generic;

namespace hobd
{

/// <summary>Sensor interface for extra data retrieval</summary>
/// <para> Extra data could be of any type, its only sensor own specification should define how to treat it</para>
public interface SensorExtra
{
    /// <summary>Typed extra values from the sensor</summary>
    object this[string key]{get;}

    /// <summary>List of available keys for extra values</summary>
    string[] ExtraKeys{get;}

}
    
}
