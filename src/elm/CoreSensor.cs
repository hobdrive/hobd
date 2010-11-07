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
        // default values
        description.Add("en", "");
    }
        
    public virtual double Value {get; protected set;}
    public long TimeStamp {get; internal set;}
    
    public virtual string ID { get; internal set; }
    
    Dictionary<string, string> name = new Dictionary<string, string>();
    public virtual string Name { get{ return name["en"];} internal set{ name["en"] = value; } }
    public virtual string GetName(string lang)
    {
        if (!name.ContainsKey(lang)) lang = "en";
        return name[lang];
    }
    internal void SetName(string lang, string val)
    {
        if (lang == "" || lang == null) lang = "en";
        if (!name.ContainsKey(lang))
            name.Add(lang, val);
        if (!name.ContainsKey("en"))
            name.Add("en", val);
    }


    Dictionary<string, string> description = new Dictionary<string, string>();
    public virtual string Description { get{ return description["en"];} internal set{ description["en"] = value; } }
    public virtual string GetDescription(string lang)
    {
        if (!description.ContainsKey(lang)) lang = "en";
        return description[lang];
    }
    internal void SetDescription(string lang, string val)
    {
        if (lang == "" || lang == null) lang = "en";
        if (!description.ContainsKey(lang))
            description.Add(lang, val);
        if (!description.ContainsKey("en"))
            description.Add("en", val);
    }

    Dictionary<string, string> units = new Dictionary<string, string>();
    public virtual string Units {
        get{
           if (!units.ContainsKey("en")) return "";
           return units["en"];
        }
        internal set{ units["en"] = value; }
    }
    public virtual string GetUnits(string lang)
    {
        if (!units.ContainsKey(lang)) lang = "en";
        if (!units.ContainsKey(lang)) return "";
        return units[lang];
    }
    internal void SetUnits(string lang, string val)
    {
        if (lang == "" || lang == null) lang = "en";
        if (!units.ContainsKey(lang))
            units.Add(lang, val);
        if (!units.ContainsKey("en"))
            units.Add("en", val);
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
