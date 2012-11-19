using System;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace hobd
{

/// <summary>
/// Information about sensor listener
/// </summary>
public class SensorListener
{
    public Sensor sensor;
    public List<Action<Sensor>> listeners = new List<Action<Sensor>>();
    public int period = 0;
    public int failures;
    public long nextReading;
#if DEBUG
    public string bt = "";
#endif
}

/// <summary>
/// Registry with active set of sensors
/// </summary>
/// Provides ability to listen to sensor changes, to fetch sensor list, etc
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

    IList<Func<string, object>> ObjectCreators = new List<Func<string, object>>();
    
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
    

    /// <summary>
    /// Provider should be either a SensorProvider full class name, or special string
    /// </summary>
    /// ecuxml://path/to/file.ecuxml
    /// for dynamic sensor definitions
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

    public object CreateObject(string clazz)
    {
        foreach(var oc in ObjectCreators)
        {
            var obj = oc(clazz);
            if (obj != null)
                return obj;
        }
        try{
            return Assembly.GetExecutingAssembly().CreateInstance(clazz);
        }catch(Exception e){
            Logger.error("SensorRegistry", "CreateObject failed: "+clazz);
        }
        return null;
    }
    
    public void RegisterObjectCreator(Func<string, object> creator)
    {        
        ObjectCreators.Add(creator);
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

    /// <summary>
    /// Returns enumeration of all the available registered sensors
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerable<Sensor>"/> list of all currently avaialable sensors
    /// </returns>
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
    
    /// <summary>
    /// Gets the sensor with the specified ID or alias
    /// </summary>
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
        
    public void DumpState()
    {
        var act = "";
        foreach(var sl in activeSensors.Values){
            act += sl.sensor.ID+" \n";
        }
        Logger.error("SensorRegistry", "========================== active: "+act);
        foreach(var sl in activeSensors.Values){
            act = sl.sensor.ID+" \n";
#if DEBUG
            act += sl.bt;
#endif
            Logger.error("SensorRegistry", "========================== active: "+act);
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
                            if (Logger.DUMP) Logger.dump("SensorRegistry", "Listener: " +l);
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
        if (Logger.DUMP && sl != null) Logger.dump("SensorRegistry", "TriggerListeners " +sensor.ID+"="+sensor.Value+" listeners:" + sl.listeners.Count + " nr="+(sl.nextReading-DateTimeMs.Now));
        if (sl != null && triggerQueue != null && (sl.nextReading == 0 || sl.nextReading <= DateTimeMs.Now))
            triggerQueue.Enqueue(sensor);
    }
    /// <summary>
    /// Triggers sensor suspend event for all sensors that supports it
    /// </summary>
    public void TriggerSuspend()
    {
        // we should wait until notify queue is empty, because otherwise some sensors may receive events after they get into suspend.
        int tqTMO = 10;
        while (triggerQueue.Count != 0 && tqTMO > 0)
        {
            Thread.Sleep(10);
            tqTMO--;
        }
        foreach( var s in sensors.Values.Where( (s) => s is IAggregatorSensor ).ToList() )
            ((IAggregatorSensor)s).Suspend();
    }
    /// <summary>
    /// Triggers sensor reset event for all sensors that supports it
    /// </summary>
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

    /// <summary>
    /// Adds listener for the specified sensor
    /// </summary>
    /// <remarks>
    /// Use period of milliseconds to
    /// update the reading. Default is 0 - means update as fast as possible
    /// </remarks>
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
            if (!sl.listeners.Contains(listener))
            {
                sl.listeners.Add(listener);
#if DEBUG
                sl.bt += Environment.StackTrace;
#endif
                sensor.NotifyAddListener(listener);
            }
            activeSensors_array = null;
	    }
	}
	
    /// <summary>
    /// alias for RemoveListener
    /// </summary>
	public void RemoveListener(string sensor, Action<Sensor> listener)
	{
	    this.RemoveListener(Sensor(sensor), listener);
	}
    /// <summary>
    /// Removes the listener for the specified sensor
    /// </summary>
	public void RemoveListener(Sensor sensor, Action<Sensor> listener)
	{
	    if (Logger.DUMP) Logger.dump("SensorRegistry", "RemoveListener "+ sensor.ID + " " + listener.ToString());
	    lock(sync_listener)
	    {
    	    if (!activeSensors.ContainsKey(sensor))
    	        return; // TODO: handle errors?
    	    SensorListener sl = activeSensors[sensor];
	        var removed = sl.listeners.RemoveAll((g) => {return g == listener;});
	        if (removed > 0)
                sensor.NotifyRemoveListener(listener);
	        if(sl.listeners.Count == 0)
	            activeSensors.Remove(sensor);
            activeSensors_array = null;
	    }
	}
	
    /// <summary>
    /// Detaches the specifed listener for all sensors
    /// </summary>
    /// <param name="listener">
    /// A <see cref="Action<Sensor>"/>
    /// </param>
	public void RemoveListener(Action<Sensor> listener)
	{
	    if (Logger.DUMP) Logger.dump("SensorRegistry", "RemoveListener " + listener.ToString());
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

    /// <summary>
    /// Passive listeners are the listeners who listen for all currently active sensors
    /// </summary>
    ///
    /// These listeners make no impact on the list of the sensors to fetch.
    ///
    /// <param name="listener">
    /// A <see cref="Action<Sensor>"/>
    /// </param>
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
