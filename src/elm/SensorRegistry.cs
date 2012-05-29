using System;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace hobd
{

public class SensorListener
{
    public Sensor sensor;
    public List<Action<Sensor>> listeners = new List<Action<Sensor>>();
    public int period = 0;
    public long nextReading;
}
/**
 * Active set of sensors.
 * Provides ability to listen to sensor changes
 */
public class SensorRegistry
{
    object sync_listener = new object();
    
    Dictionary<string, Sensor> sensors = new Dictionary<string, Sensor>();
    Dictionary<string, Sensor> sensorNames = new Dictionary<string, Sensor>();

    Dictionary<Sensor, SensorListener> activeSensors = new Dictionary<Sensor, SensorListener>();
    SensorListener[] activeSensors_array = null;

    List<Action<Sensor>> PassiveListeners = new List<Action<Sensor>>();

    Thread listenThread;
    Queue<Sensor> triggerQueue = new Queue<Sensor>();
    public int QueueSize { get{ return triggerQueue.Count; } }
    
    public IDictionary<string, string> VehicleParameters {get;set;}

    public int ProtocolId{get;set;}
    
    public SensorRegistry()
    {
        VehicleParameters = new Dictionary<string,string>();

        listenThread = new Thread(this.ListenerHandler);
        listenThread.Priority = ThreadPriority.AboveNormal;
        listenThread.Start();
    }
    public void Deactivate()
    {
        if (listenThread != null)
        {
            triggerQueue = null;
            listenThread.Join(1000);
            listenThread = null;
        }
    }
    
    /**
     Provider should be either a SensorProvider full class name, or special string
     ecuxml://path/to/file.ecuxml
     for dynamic sensor definitions
     */
    public void RegisterProvider(string provider)
    {
        if (provider.StartsWith("ecuxml://")){
            provider = provider.Replace("ecuxml://", "");
            var prov = new ECUXMLSensorProvider(provider);
            this.RegisterProvider(prov);
        }else{
            this.RegisterProvider( (SensorProvider)Assembly.GetExecutingAssembly().CreateInstance(provider));
        }
    }

    public void RegisterProvider(SensorProvider provider)
	{
	    provider.Activate(this);
	}

    public void Add(Sensor sensor)
    {
        sensors.Remove(sensor.ID);
        sensors.Add(sensor.ID, sensor);
        if (sensor.Name != null)
        {
            sensorNames.Remove(sensor.Name);
            sensorNames.Add(sensor.Name, sensor);
        }
        sensor.SetRegistry(this);
    }

    public void AddAlias(Sensor sensor, string alias)
    {
        sensorNames.Remove(alias);
        sensorNames.Add(alias, sensor);
        sensor.SetRegistry(this);
    }

    public void Remove(Sensor sensor)
    {
        sensors.Remove(sensor.ID);

        if (sensor.Name != null)
        {
            sensorNames.Remove(sensor.Name);
        }
    }

    /**
     * Returns enumeration of all the available registered sensors
     */
    public IEnumerable<Sensor> EnumerateSensors()
    {
        return sensors.Values;
    }

    public IEnumerable<Sensor> Sensors
    {
        get {
            return sensors.Values;
        }
    }
    
    /**
     * Gets the sensor with the specified ID or alias
     */
    public Sensor Sensor(string id)
    {
        Sensor value;
        if (sensors.TryGetValue(id, out value))
            return value;
        else if (sensorNames.TryGetValue(id, out value))
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
    
    void ListenerHandler()
    {
        while(triggerQueue != null)
        {
            if (triggerQueue.Count == 0)
            {
                Thread.Sleep(10);
            }else{
                SensorListener sl = null;
                var sensor = triggerQueue.Dequeue();
                if (sensor != null)
                {
                    activeSensors.TryGetValue(sensor, out sl);
                }
                if (sl != null) {
                    sl.nextReading = DateTimeMs.Now + sl.period;
                    if (Logger.DUMP) Logger.dump("SensorRegistry", "ListenerHandler " +sensor.ID+" "+sl.nextReading);
                    foreach(Action<Sensor> l in sl.listeners.ToArray()) {
                        try{
                            l(sensor);
                        }catch(Exception e)
                        {
                            Logger.error("SensorRegistry", "Listener fail on: "+sensor.ID, e);
                        }
                    }
                    foreach(Action<Sensor> l in PassiveListeners.ToArray()) {
                        try{
                            l(sensor);
                        }catch(Exception e)
                        {
                            Logger.error("SensorRegistry", "Passive listener fail on: "+sensor.ID, e);
                        }
                    }
                }
            }
        }
    }


    public void TriggerListeners(Sensor sensor)
    {
        if (sensor == null)
            throw new ArgumentNullException();
        SensorListener sl = null;
        activeSensors.TryGetValue(sensor, out sl);
        if (Logger.DUMP && sl != null) Logger.dump("SensorRegistry", "TriggerListeners " +sensor.ID+"="+sensor.Value+" nr="+sl.nextReading);
        if (sl != null && triggerQueue != null && (sl.nextReading == 0 || sl.nextReading <= DateTimeMs.Now))
            triggerQueue.Enqueue(sensor);
    }
    /**
     * Triggers sensor suspend event for all sensors that supports it
     */
    public void TriggerSuspend()
    {
        foreach( var s in sensors.Values.Where( (s) => s is IAggregatorSensor ).ToList() )
            ((IAggregatorSensor)s).Suspend();
    }
    /**
     * Triggers sensor reset event for all sensors that supports it
     */
    public void TriggerReset()
    {
        foreach( var s in sensors.Values.Where( (s) => s is IAggregatorSensor ).ToList() )
            ((IAggregatorSensor)s).Reset();
    }

	public void AddListener(string sensor, Action<Sensor> listener)
	{
	    this.AddListener(sensor, listener, 0);
	}
	public void AddListener(string sensor, Action<Sensor> listener, int period)
	{
        if (Sensor(sensor) == null)
            throw new NullReferenceException("No such sensor: "+sensor);
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
	    if (Logger.DUMP) Logger.dump("SensorRegistry", "AddListener "+ sensor.ID + " " + listener.ToString() + " " + period);
	    lock(sync_listener)
	    {
            if (sensor == null)
                throw new NullReferenceException("Null sensor");
    	    SensorListener sl = null;
    	    try{
    	        sl = activeSensors[sensor];
    	    }catch(KeyNotFoundException){
    	        sl = new SensorListener();
    	        sl.sensor = sensor;
    	        sl.period = period;
    	        activeSensors.Add(sensor, sl);
    	    }
    	    if (sl.period > period){
    	        sl.period = period;
    	        sl.nextReading = 0;
    	    }
            sl.listeners.Add(listener);
            sensor.NotifyAddListener(listener);
            activeSensors_array = null;
	    }
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
	    lock(sync_listener)
	    {
    	    SensorListener sl = activeSensors[sensor];
	        var removed = sl.listeners.RemoveAll((g) => {return g == listener;});
	        if (removed > 0)
                sensor.NotifyRemoveListener(listener);
	        if(sl.listeners.Count == 0)
	            activeSensors.Remove(sensor);
            activeSensors_array = null;
	    }
	}
	
    /**
     * Detaches the specifed listener for all sensors
     */
	public void RemoveListener(Action<Sensor> listener)
	{
	    lock(sync_listener)
	    {
	        foreach (var sl in activeSensors.Values.ToArray()) {
                var removed = sl.listeners.RemoveAll((g) => {return g == listener;});
    	        if (removed > 0)
                    sl.sensor.NotifyRemoveListener(listener);
    	        if(sl.listeners.Count == 0)
    	            activeSensors.Remove(sl.sensor);
	        }
    	    activeSensors_array = null;
	    }
	}

	/**
	 * Passive listeners are the listeners who listen for all currently active sensors.
	 * These listeners make no impact on the list of the sensors to fetch.
	 */
	public void AddPassiveListener(Action<Sensor> listener)
	{
	    if (!PassiveListeners.Contains(listener))
	        PassiveListeners.Add(listener);
	}
	public void RemovePassiveListener(Action<Sensor> listener)
	{
	    if (PassiveListeners.Contains(listener))
	        PassiveListeners.Remove(listener);
	}

}

}
