using System;
using System.Collections.Generic;

namespace hobd
{

public class OBD2Sensor : CoreSensor
{
    public Func<OBD2Sensor, double> obdValue;
    protected byte[] dataraw;
    protected int data_offset;
    
    public OBD2Sensor()
    {
    }

    public int Command { get; set; }
    
    public override double Value { get; protected set; }
    
    public virtual string RawCommand {
        get{
            return "01" + this.Command.ToString("X2");
        }
    }

    public static byte to_h(byte a)
    {
        if (a >= 0x30 && a <= 0x39) return (byte)(a-0x30);
        if (a >= 0x41 && a <= 0x46) return (byte)(a+10-0x41);
        if (a >= 0x61 && a <= 0x66) return (byte)(a+10-0x61);
        return 255;
    }

    public virtual bool SetRawValue(byte[] msg)
    {
        var msgraw = new List<byte>();
        
        // parse reply
        for(int i = 0; i < msg.Length; i++)
        {
            var a = msg[i];
            if (a == ' ' || a == '\r' || a == '\n')
                continue;
            if (i+1 >= msg.Length)
                break;
            i++;
            var b = msg[i];
            a = to_h(a);
            b = to_h(b);
            if (a > 0x10 || b > 0x10)
                continue;
            
            msgraw.Add((byte)((a<<4) + b));
            
        }
        
        byte[] dataraw = msgraw.ToArray();

        return this.SetValue(dataraw);
    }

    public virtual bool SetValue(byte[] dataraw)
    {
        data_offset = 0;
        while(data_offset < dataraw.Length-1 && !(dataraw[data_offset] == 0x41 && dataraw[data_offset+1] == this.Command))
        {
            data_offset++;
        }
        if (data_offset >= dataraw.Length-1)
            return false;

        this.dataraw = dataraw;

        try{
            this.Value = obdValue(this);
        }catch(Exception e){
            string r = "";
            for (int i = 0; i < dataraw.Length; i++)
               r += dataraw[i].ToString("X2") + " ";
            Logger.error("OBD2Sensor", "Fail parsing sensor value: " + this.ID + " " + r, e);
            return false;
        }
        this.TimeStamp = DateTimeMs.Now;
        registry.TriggerListeners(this);
        return true;
    }
    
    public double get(int idx)
    {
        return dataraw[data_offset+2+idx];
    }

    public double get_bit(int idx, int bit)
    {
        return (dataraw[data_offset+2+idx] & (1<<bit)) != 0 ? 1 : 0;
    }

    public double getab()
    {
        return get(0)*256 + get(1);
    }
    public double getcd()
    {
        return get(2)*256 + get(3);
    }
    
}

}
