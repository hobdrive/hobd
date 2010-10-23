using System;
using System.Collections.Generic;
using System.Linq;

namespace hobd
{

public class SensorListener
{
    public Sensor sensor;
    public List<Action<Sensor>> listeners = new List<Action<Sensor>>();
    public int period = 0;
}
/**
 * Active set of sensors.
 * Provides ability to listen to sensor changes
 */
public class SensorRegistry
{
    
    Dictionary<string, Sensor> sensors = new Dictionary<string, Sensor>();
    Dictionary<Sensor, SensorListener> activeSensors = new Dictionary<Sensor, SensorListener>();
    SensorListener[] activeSensors_array = null;
    
    public IDictionary<string, string> VehicleParameters {get;set;}
    
    public SensorRegistry()
    {
    }
    
    public void RegisterProvider(SensorProvider provider)
	{
	    provider.Activate(this);
	}

    public void Add(Sensor sensor)
    {
        sensors.Remove(sensor.ID);
        sensors.Add(sensor.ID, sensor);
        foreach (string alias in sensor.Aliases){
            sensors.Remove(alias);
            sensors.Add(alias, sensor);
        }
        sensor.SetRegistry(this);
    }
    
    /**
     * Gets the sensor with the specified ID or alias
     */
    public Sensor Sensor(string id)
    {
        Sensor value;
        if (sensors.TryGetValue(id, out value))
            return value;
        else
            return null;
    }
    public SensorListener[] ActiveSensors
    {
        get{
            if (activeSensors_array == null){
                activeSensors_array = new SensorListener[activeSensors.Values.Count];
                activeSensors.Values.CopyTo(activeSensors_array, 0);
            }
            return activeSensors_array;
        }
    }
    
    public void TriggerListeners(Sensor sensor)
    {
        SensorListener sl = null;
        activeSensors.TryGetValue(sensor, out sl);
        if (sl != null){
            foreach(Action<Sensor> l in sl.listeners){
                try{
                    l(sensor);
                }catch(Exception e)
                {
                    if (Logger.ERROR) Logger.error("SensorRegistry", "Listener fail on: "+sensor.ID, e);
                }
            }
        }
    }
    /**
     * Triggers sensor suspend event for all sensors that supports it
     */
    public void TriggerSuspend()
    {
        foreach( var s in sensors.Values.Where( (s) => s is IAccumulatorSensor ) )
            ((IAccumulatorSensor)s).Suspend();
    }
    /**
     * Triggers sensor reset event for all sensors that supports it
     */
    public void TriggerReset()
    {
        foreach( var s in sensors.Values.Where( (s) => s is IAccumulatorSensor ) )
            ((IAccumulatorSensor)s).Reset();
    }

	public void AddListener(string sensor, Action<Sensor> listener)
	{
	    this.AddListener(Sensor(sensor), listener, 0);
	}
	public void AddListener(string sensor, Action<Sensor> listener, int period)
	{
	    this.AddListener(Sensor(sensor), listener, period);
	}
	public void AddListener(Sensor sensor, Action<Sensor> listener)
	{
	    this.AddListener(sensor, listener, 0);
	}
    /**
     * Adds listener for the specified sensor. Use period of milliseconds to
     * update the reading. Default is 0 - means update as fast as possible
     */
	public void AddListener(Sensor sensor, Action<Sensor> listener, int period)
	{
	    if (sensor == null)
	        throw new NullReferenceException("No such sensor");
	    SensorListener sl = null;
	    try{
	        sl = activeSensors[sensor];
	    }catch(KeyNotFoundException){
	        sl = new SensorListener();
	        sl.sensor = sensor;
	        sl.period = period;
	        activeSensors.Add(sensor, sl);
	    }
	    if (sl.period > period)
	        sl.period = period;
        sl.listeners.Add(listener);
        activeSensors_array = null;
	}
	
    /**
     * alias for RemoveListener
     */
	public void RemoveListener(string sensor, Action<Sensor> listener)
	{
	    this.RemoveListener(Sensor(sensor), listener);
	}
    /**
     * Removes the listener for the specified sensor
     */
	public void RemoveListener(Sensor sensor, Action<Sensor> listener)
	{
	    SensorListener sl = activeSensors[sensor];
	    if (sl != null)
	    {
	        sl.listeners.RemoveAll((g) => {return g == listener;});
	        if(sl.listeners.Count == 0)
	            activeSensors.Remove(sensor);
	    }
	    activeSensors_array = null;
	}
	
    /**
     * Detaches the specifed listener for all sensors
     */
	public void RemoveListener(Action<Sensor> listener)
	{
        foreach (var sl in activeSensors.Values) {
            sl.listeners.RemoveAll((g) => {return g == listener;});
        }
	    activeSensors_array = null;
	}

}

}
