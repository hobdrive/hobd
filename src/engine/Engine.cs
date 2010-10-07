using System;

namespace hobd
{

public class Engine
{

    protected IStream stream;
    protected bool active = false;
    protected string url;
    
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
    
    private SensorRegistry registry;
    
    public SensorRegistry Registry
    {
        set{
            if (active) throw new InvalidOperationException("Can't change Registry on active Engine");
            if (value == null) throw new InvalidOperationException("Can't set null Registry");
            registry = value;
        }
        get{
            return registry;
        }
    }
    
}

}