using System;

namespace hobd.elm.injector
{
    
public class InjectorSensors : SensorProvider
{

    public InjectorSensors()
    {
    }
    
    public string GetName()
    {
        return "InjectorSensors";
    }

    public string GetDescription()
    {
        return "Injector derived data sensors";
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