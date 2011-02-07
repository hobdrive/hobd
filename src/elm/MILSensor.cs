using System;
using System.Text;
using System.Collections.Generic;

namespace hobd
{

/**
 * MIL sensor reads all raised DTC codes and provides alternative interface to fetch them.
 * It works as a regular sensor, however the recommended read interval should be of course relatively big (MILSensor.ReadInterval)
 * Read DTCs are fetched via MILValue property which returs array of strings with detected DTCs.
 */
public class MILSensor : OBD2Sensor
{

    public MILSensor()
    {
    }

    public const int ReadInterval = 5*60*1000;

    public override double Value { get{ return 0; } }

    protected string[] mil_value;
    
    public override string RawCommand {
        get{
            return "03";
        }
    }

    public override bool SetValue(byte[] dataraw)
    {
        /*
        if (dataraw.Length < 1)
            return false;
        */
        this.dataraw = dataraw;
        /*
        if (Logger.TRACE){
            string r = "";
            for (int i = 0; i < dataraw.Length; i++)
               r += dataraw[i].ToString("X2") + " ";
            Logger.trace("MIL", "dataraw: "+r);
        }
        */
        this.mil_value = null;
        this.TimeStamp = DateTimeMs.Now;
        registry.TriggerListeners(this);
        return true;
    }

    public virtual string[] MILValue{
        get{
            if (mil_value == null && dataraw != null)
            {
                var l = new List<string>();

                int idx = 0;

                if (registry.ProtocolId >= 6)
                {
                    int len = 0;
                    if ((dataraw[0]&0xF0) != 0x40)
                        idx++;
                    while(idx < dataraw.Length-1)
                    {
                        // reply?
                        if (len == 0)
                        {
                            if ((dataraw[idx]&0xF0) == 0x40)
                            {
                                idx++;
                                len = dataraw[idx];
                                idx++;
                                continue;
                            }else{
                                idx++;
                                continue;
                            }
                        }
                        // codes
                        var a = dataraw[idx];
                        var b = dataraw[idx+1];
                        idx+=2;

                        if (a == 0 && b == 0)
                            continue;

                        string mil = (new string[]{"P", "C", "B", "U"})[a>>6];

                        mil += (char)(0x30 + ((a>>4)&0x3));
                        mil += (char)(0x30 + ((a>>0)&0xF));
                        
                        mil += (char)(0x30 + ((b>>4)&0xF));
                        mil += (char)(0x30 + ((b>>0)&0xF));
                        l.Add(mil);
                        len--;
                    }
                }
                else{
                    for(; idx < dataraw.Length-1; idx +=2)
                    {
                        if (idx % 7 == 0)
                            idx++;

                        var a = dataraw[idx];
                        var b = dataraw[idx+1];

                        if (a == 0 && b == 0)
                            continue;

                        string mil = (new string[]{"P", "C", "B", "U"})[a>>6];

                        mil += (char)(0x30 + ((a>>4)&0x3));
                        mil += (char)(0x30 + ((a>>0)&0xF));
                        
                        mil += (char)(0x30 + ((b>>4)&0xF));
                        mil += (char)(0x30 + ((b>>0)&0xF));
                        l.Add(mil);
                    }
                }

                mil_value = l.ToArray();
            }
            return mil_value;
        }
    }

}

}
