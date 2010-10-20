using System;
using System.Collections.Generic;
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
    private DateTime stateTS;
    Thread worker;
    
    public const string ST_INIT = "INIT";
    public const string ST_ATZ = "ATZ";
    public const string ST_ATE0 = "ATE0";
    public const string ST_ATL0 = "ATL0";
    public const string ST_SENSOR = "SENSOR";
    public const string ST_SENSOR_ACK = "SENSOR_ACK";
    public const string ST_ERROR = "ERROR";

    public string State {get; private set;}
    public string Error {get; private set;}
    
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
        Logger.trace("SendCommand:" + command);
        byte[] arr = Encoding.ASCII.GetBytes(command+"\r");
        stream.Write(arr, 0, arr.Length);
    }
    
    int currentSensorIndex = -1;
    SensorListener currentSensorListener = null;
    DateTime[] nextReadings = null;
    
    void SetState(string state2)
    {
        
        State = state2;
        stateTS = DateTime.Now;
        
        Logger.trace("OBD2Engine.SetState " + State);
        
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
                SendCommand("");
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
                        Logger.trace("SENSOR: " + currentSensorListener.sensor.ID);
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
    
    string versionInfo = "";

    byte to_h(byte a)
    {
        if (a >= 0x30 && a <= 0x39) return (byte)(a-0x30);
        if (a >= 0x41 && a <= 0x46) return (byte)(a+10-0x41);
        if (a >= 0x61 && a <= 0x66) return (byte)(a+10-0x61);
        return a;
    }
    
    void HandleReply(byte[] msg)
    {
        string smsg = Encoding.ASCII.GetString(msg);
        if (Logger.TRACE) Logger.trace(State + ": " + smsg.Trim());
        
        switch(State){
            case ST_INIT:
                versionInfo = smsg.Trim();
                break;
            case ST_ATZ:
                SetState(ST_ATE0);
                break;
            case ST_ATE0:
                if (smsg.Contains("OK"))
                {
                    SetState(ST_ATL0);
                }
                break;
            case ST_ATL0:
                SetState(ST_SENSOR);
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
                osensor.data_raw = msgraw.ToArray();
                nextReadings[currentSensorIndex] = DateTime.Now.AddMilliseconds(currentSensorListener.period);
                
                if (osensor.data_raw.Length > 1 && osensor.data_raw[0] == 0x41 && osensor.data_raw[1] == osensor.Command)
                {
                    foreach(Action<Sensor> l in currentSensorListener.listeners){
                        try{
                            l(currentSensorListener.sensor);
                        }catch(Exception e)
                        {
                            if (Logger.TRACE) Logger.trace("Listener fail on: "+osensor.ID+ ": "+e.Message);
                        }
                    }
                }
                SetState(ST_SENSOR);
                break;
        }
    }
    
    byte[] buffer = new byte[256];
    int position = 0;
    
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
            data = null;
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
        
            HandleState();
            
            var diff_ms = DateTime.Now.Subtract(stateTS).TotalMilliseconds;
            // Restart the hanged connection after two seconds
            if (diff_ms > 2000) {
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
