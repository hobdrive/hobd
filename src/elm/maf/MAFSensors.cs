using System;
using hobd;

namespace hobd.elm.maf
{
    
public class MAFSensors : SensorProvider
{

    public MAFSensors()
    {
    }
    
    public string GetName()
    {
        return "MAFSensors";
    }

    public string GetDescription()
    {
        return "MAF derived data sensors";
    }
    
    public string GetDescription(string lang)
    {
        return GetDescription();
    }

    public void Activate(SensorRegistry registry)
    {
        registry.Add(new LitersPerHourSensor());
    }
}
}