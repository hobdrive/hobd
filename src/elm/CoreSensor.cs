using System;
using System.Collections.Generic;

namespace hobd
{

/// <summary>Implementation of a simple sensor</summary>
/// Sensor value could only be changed by derived classses
public class CoreSensor : Sensor
{
    protected SensorRegistry registry;
    protected int listenerCount = 0;
        
    /// default constructor
    public CoreSensor()
    {
    }
        
    protected double value;
	/// <summary>
	/// Setting up the value automatically sets the sensor Valid to <c>true</c>
	/// </summary>
    public virtual double Value {
        get{
            return this.value;
        }
        protected set{
            this.value = value;
            this.Valid = true;
        }
    }
    /// <inheritdoc/>
    public long TimeStamp {get; set;}
    
    /// <inheritdoc/>
    public virtual bool Valid { get; protected set; }

    /// <inheritdoc/>
    public virtual bool Available { get; protected set; }

    /// <inheritdoc/>
    public virtual string ID { get; set; }
    
    /// <inheritdoc/>
    public virtual string Name { get; set; }

    /// <inheritdoc/>
    public virtual string Units {
        get;
        set;
    }
    
    /// <inheritdoc/>
    public virtual void SetRegistry(SensorRegistry registry) {
        this.registry = registry;
    }

    /// <inheritdoc/>
    public virtual void DetachRegistry() {
        this.registry = null;
    }
        
    /// <inheritdoc/>
    public virtual bool Active{
        get{ return listenerCount != 0; }
    }
    
    /// <inheritdoc/>
    public virtual void NotifyAddListener(Action<Sensor> listener)
    {
        listenerCount++;
        if (listenerCount == 1)
            Activate();
    }
    
    /// <inheritdoc/>
    public virtual void NotifyRemoveListener(Action<Sensor> listener)
    {
        listenerCount--;
        if (listenerCount == 0)
            Deactivate();
    }

    /// <summary>
    /// Helper method to help sensor know when it is activated
    /// </summary>
    protected virtual void Activate()
    {
    }
    /// <summary>
    /// Helper method to help sensor know when it is deactivated
    /// </summary>
    protected virtual void Deactivate()
    {
    }
}

}
