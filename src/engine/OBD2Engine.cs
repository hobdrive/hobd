using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace hobd
{

/// <summary>
/// Description of OBD2Engine.
/// </summary>
public class OBD2Engine : Engine
{
    private bool thread_active = false;
    private DateTime stateTS, lastReceiveTS;
    Thread worker;

    int currentSensorIndex = -1;
    SensorListener currentSensorListener = null;
    DateTime[] nextReadings = null;

    string versionInfo = "";
    byte[] buffer = new byte[256];
    int position = 0;

    public const int ErrorThreshold = 20;
    
    public const string ST_INIT = "INIT";
    public const string ST_ATZ = "ATZ";
    public const string ST_ATE0 = "ATE0";
    public const string ST_ATL0 = "ATL0";
    public const string ST_SENSOR = "SENSOR";
    public const string ST_SENSOR_ACK = "SENSOR_ACK";
    public const string ST_ERROR = "ERROR";

    string[] dataErrors = new string[]{ "NO DATA", "DATA ERROR" };

    int subsequentErrors = 0;

    public string State {get; private set;}
    
    public OBD2Engine()
    {
    }
    
    public override void Activate()
    {
        base.Activate();

        worker = new Thread(this.Run);
        worker.Start();
    }
    
    void PurgeStream()
    {
        Thread.Sleep(50);
        while(stream.HasData())
        {
            stream.Read();
        }
    }
    
    void SendCommand(string command)
    {
        Logger.trace("OBD2Engine", "SendCommand:" + command);
        byte[] arr = Encoding.ASCII.GetBytes(command+"\r");
        stream.Write(arr, 0, arr.Length);
    }
    void SendRaw(string command)
    {
        Logger.trace("OBD2Engine", "SendRaw:" + command);
        byte[] arr = Encoding.ASCII.GetBytes(command);
        stream.Write(arr, 0, arr.Length);
    }
        
    void SetState(string state2)
    {
        
        State = state2;
        stateTS = DateTime.Now;
        
        Logger.trace("OBD2Engine", " -> " + State);
        
        switch(State){
            case ST_INIT:
                fireStateNotify(STATE_INIT);
                try{
                    stream.Close();
                    stream.Open(url);
                }catch(Exception e){
                    Error = e.ToString();
                    Logger.trace(Error);
                    SetState(ST_ERROR);
                    break;
                }
                PurgeStream();
                SetState(ST_ATZ);
                break;
            case ST_ATZ:
                SendCommand("ATZ");
                break;
            case ST_ATE0:
                SendCommand("ATE0");
                break;
            case ST_ATL0:
                SendCommand("ATL0");
                break;
            case ST_SENSOR:
                
                fireStateNotify(STATE_READ);

                var sls = Registry.ActiveSensors;
                
                if (sls.Length == 0)
                {
                    break;
                }
                
                currentSensorIndex++;
                if (currentSensorIndex >= sls.Length)
                    currentSensorIndex = 0;
                
                int scanSensorIndex = currentSensorIndex;
                
                while (true)
                {
                        
                    currentSensorListener = sls[currentSensorIndex];
                    
                    if (nextReadings == null || nextReadings.Length < sls.Length){
                        nextReadings = new DateTime[sls.Length];
                    }
                    DateTime nr = nextReadings[currentSensorIndex];
                    
                    if (nr == null || nr < DateTime.Now){
                        Logger.trace("OBD2Engine", " ----> " + currentSensorListener.sensor.ID);
                        if (currentSensorListener.sensor is OBD2Sensor){
                            var osensor = (OBD2Sensor)currentSensorListener.sensor;
                            SendCommand("01" + osensor.Command.ToString("X2"));
                            SetState(ST_SENSOR_ACK);
                        }
                        break;
                    }
                    
                    currentSensorIndex++;
                    if (currentSensorIndex >= sls.Length)
                        currentSensorIndex = 0;
                    if (currentSensorIndex == scanSensorIndex)
                        break;
                }
                break;
            case ST_SENSOR_ACK:
                fireStateNotify(STATE_READ_DONE);
                break;
            case ST_ERROR:
                fireStateNotify(STATE_ERROR);
                break;                
        }
    }
    
    byte to_h(byte a)
    {
        if (a >= 0x30 && a <= 0x39) return (byte)(a-0x30);
        if (a >= 0x41 && a <= 0x46) return (byte)(a+10-0x41);
        if (a >= 0x61 && a <= 0x66) return (byte)(a+10-0x61);
        return a;
    }
    
    void HandleReply(byte[] msg)
    {
        string smsg = Encoding.ASCII.GetString(msg, 0, msg.Length);
        if (Logger.TRACE) Logger.trace("OBD2Engine", "HandleReply: " + smsg.Trim());
        
        switch(State){
            case ST_INIT:
                versionInfo = smsg.Trim();
                break;
            case ST_ATZ:
                if (smsg.Contains("ATZ"))
                {
                    SetState(ST_ATE0);
                }else{
                    SendCommand("ATZ");
                }
                break;
            case ST_ATE0:
                if (smsg.Contains("OK"))
                {
                    SetState(ST_ATL0);
                }
                break;
            case ST_ATL0:
                if (smsg.Contains("OK"))
                {
                    SetState(ST_SENSOR);
                }
                break;
            case ST_SENSOR_ACK:
                
                var msgraw = new List<byte>();
                
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
                        break;
                    
                    msgraw.Add((byte)((a<<4) + b));
                    
                }
                
                var osensor = (OBD2Sensor)currentSensorListener.sensor;
                byte[] dataraw = msgraw.ToArray();
                
                nextReadings[currentSensorIndex] = DateTime.Now.AddMilliseconds(currentSensorListener.period);
                
                if (dataraw.Length > 1 && dataraw[0] == 0x41 && dataraw[1] == osensor.Command)
                {
                    osensor.SetValue(dataraw);
                    subsequentErrors = 0;
                }else{
                	string error = dataErrors.FirstOrDefault(e => smsg.Contains(e));
                	if (error != null)
                	{
                	    // increase period for this 'bad' sensor
                	    if (subsequentErrors == 0)
                	    {
                	        currentSensorListener.period = (currentSensorListener.period +100) * 2;
                	    }
                	    subsequentErrors++;
                	}
                }
                if (subsequentErrors > ErrorThreshold) {
                    Logger.error("OBD2Engine", "Connection error threshold");
                    SetState(ST_INIT);
                }
                SetState(ST_SENSOR);
                break;
        }
    }
        
    void HandleState()
    {
        
        if (State == ST_SENSOR)
        {
            Thread.Sleep(50);
            SetState(ST_SENSOR);
            return;
        }
        
        if (stream.HasData())
        {
            byte[] data = stream.Read();
            if (position + data.Length < buffer.Length)
            {
                Array.Copy(data, 0, buffer, position, data.Length);
                position = position + data.Length;
            }else{
                position = 0;
            }
            if (Logger.DUMP) Logger.dump("OBD2Engine", "BUFFER: "+Encoding.ASCII.GetString(buffer, 0, position));
            data = null;
            lastReceiveTS = DateTime.Now;
        }

        if (position == 0)
        {
            Thread.Sleep(50);
            return;
        }
        
        for(int isearch = 0; isearch < position; isearch++)
        {
            // end of reply found
            if (buffer[isearch] == '>'){
                byte[] msg = new byte[isearch];
                Array.Copy(buffer, 0, msg, 0, isearch);
                isearch++;
                Array.Copy(buffer, isearch, buffer, 0, position-isearch);
                position = position-isearch;
                // handle our extracted message
                HandleReply(msg);
                break;
            }
        }
    }
    
    void Run()
    {
        thread_active = true;
        
        SetState(ST_INIT);
        
        while(this.active){
        
            try{
                HandleState();
            }catch(Exception e){
                Logger.error("OBD2Engine", "Run exception", e);
                SetState(ST_INIT);
            }
            
            // No reply. Ping the connection. Only OBDSim bugs?
            if (DateTime.Now.Subtract(lastReceiveTS).TotalMilliseconds > 500 && State != ST_ERROR) {
                //SendCommand("AT");
                SendRaw(" ");
                lastReceiveTS = DateTime.Now;
            }
            // Restart the hanged connection after N seconds
            var diff_ms = DateTime.Now.Subtract(stateTS).TotalMilliseconds;
            if (diff_ms > 3000) {
                // If ERROR, wait for a longer period before retrying
                if (this.State != ST_ERROR || diff_ms > 10000)
                {
                    SetState(ST_INIT);
                }
			  }
            
        }
        thread_active = false;
    }
    
    public override void Deactivate()
    {
        base.Deactivate();
        int counter = 10;
        // TODO! WTF???
        worker.Abort();
        worker.Join();
        stream.Close();
        while(thread_active && counter > 0){
            Thread.Sleep(100);
            counter--;
        }
    }
    
    
    
}

}
