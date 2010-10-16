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
    double GetValue();
    
    string ID{get;}

    string Name{get;}
    
    string Description{get;}
    
    string GetDescription(string lang);
    
    string Units{get;}
    
    IEnumerable<string> Aliases{get;}
    
}
    
}
