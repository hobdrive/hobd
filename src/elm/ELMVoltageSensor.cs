using System;
using System.Text;
using System.Collections.Generic;

namespace hobd.elm
{

public class ELMVoltageSensor : OBD2Sensor
{

    public ELMVoltageSensor()
    {
    }

    public override string RawCommand {
        get{
            return "ATRV";
        }
    }

    public override bool SetRawValue(byte[] msg)
    {
        var val = Encoding.ASCII.GetString(msg, 0, msg.Length);
        try{
            val = val.Replace("V", "");
            this.Value = double.Parse(val, UnitsConverter.DefaultNumberFormat);
            this.TimeStamp = DateTimeMs.Now;
            registry.TriggerListeners(this);
        }catch(Exception e){
            Logger.error("ELMVoltageSensor", "data: "+val, e);
        }
        return true;
    }
}

}
