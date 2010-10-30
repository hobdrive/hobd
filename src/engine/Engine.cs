using System;

namespace hobd
{

public class Engine
{

    protected IStream stream;
    protected bool active = false;
    protected string url;
    
    public const int STATE_INIT = 0;
    public const int STATE_READ = 1;
    public const int STATE_READ_DONE = 2;
    public const int STATE_ERROR = 3;
    
    public event Action<int> StateNotify;

    /**
     * Reason of the last error condition
     */
    public string Error {get; protected set;}
    
    public Engine()
    {
    
    }
    
    public virtual void Init(IStream stream, string url)
    {
        if (active) throw new InvalidOperationException("Can't Init on active Engine");
        this.stream = stream;
        this.url = url;
    }
    
    public virtual void Activate()
    {
        if (Registry == null)
            throw new InvalidOperationException("Can't Init without SensorRegistry");
        active = true;
    }
    
    public virtual void Deactivate()
    {
        if(stream != null){
            stream.Close();
        }
        active = false;
    }
    
    public virtual bool IsActive()
    {
        return active;
    }
    
    protected void fireStateNotify(int state)
    {
        if (this.StateNotify != null)
            this.StateNotify(state);
    }
    
    void OnEngineReset(int state)
    {
        if (state == STATE_INIT){
            // Tells sensor that engine operation was delayed
            // TriggerReset is possible too, but is a user-controlled action
            Registry.TriggerSuspend();
        }
    }
    
    private SensorRegistry registry;
    
    public SensorRegistry Registry
    {
        set{
            if (active) throw new InvalidOperationException("Can't change Registry on active Engine");
            if (value == null) throw new InvalidOperationException("Can't set null Registry");
            registry = value;
            StateNotify += OnEngineReset;
        }
        get{
            return registry;
        }
    }
    
}

}