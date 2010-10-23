using System;

namespace hobd
{
    
public class CommonSensors : SensorProvider
{

    public CommonSensors()
    {
    }
    
    public string GetName()
    {
        return "CommonSensors";
    }

    public string GetDescription()
    {
        return "Common derived data sensors";
    }
    
    public string GetDescription(string lang)
    {
        return GetDescription();
    }

    public void Activate(SensorRegistry registry)
    {
        registry.Add(new DistanceSensor());
    }
}
}