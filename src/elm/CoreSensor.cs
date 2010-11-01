using System;
using System.Collections.Generic;

namespace hobd
{

public class CoreSensor : Sensor
{
    protected SensorRegistry registry;
    protected int listenerCount = 0;
        
    public CoreSensor()
    {
        this.Aliases = new List<string>();
    }
        
    public virtual double Value {get; protected set;}
    public long TimeStamp {get; internal set;}
    
    public virtual string ID { get; internal set; }
    public virtual string Name { get; internal set; }
    public virtual string Description { get; internal set; }
    public virtual string Units { get; internal set; }
    public virtual string GetDescription(string lang)
    {
        return Description;
    }
    
    public virtual IEnumerable<string> Aliases{ get; internal set; }

    public virtual void SetRegistry(SensorRegistry registry) {
        this.registry = registry;
    }
        
    
    public virtual void NotifyAddListener(Action<Sensor> listener)
    {
        listenerCount++;
    }
    
    public virtual void NotifyRemoveListener(Action<Sensor> listener)
    {
        listenerCount--;
    }
}

}
