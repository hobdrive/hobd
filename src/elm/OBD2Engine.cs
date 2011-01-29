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
    private DateTime stateTS;
    private long lastReceiveTS;
    Thread worker;

    int currentSensorIndex = -1;
    SensorListener currentSensorListener = null;
    long[] nextReadings = null;

    string versionInfo = "";
    byte[] buffer = new byte[256];
    int position = 0;

    List<string> extraInitCommands = new List<string>();
    int extraInitIndex;
    
    public string Protocol{get; protected set;}

    public const int ErrorThreshold = 10;
    
    public const string ST_INIT = "INIT";
    public const string ST_ATZ = "ATZ";
    public const string ST_ATE0 = "ATE0";
    public const string ST_ATL0 = "ATL0";
    public const string ST_EXTRAINIT = "EXTRAINIT";
    public const string ST_SENSOR_INIT = "SENSOR_INIT";
    public const string ST_QUERY_PROTOCOL = "QUERY_PROTOCOL";
    public const string ST_SENSOR = "SENSOR";
    public const string ST_SENSOR_ACK = "SENSOR_ACK";
    public const string ST_ERROR = "ERROR";

    string[] dataErrors = new string[]{ "NO DATA", "DATA ERROR", "BUS BUSY", "BUS ERROR", "CAN ERROR", "LV RESET", "UNABLE TO CONNECT" };

    int subsequentErrors = 0;

    public string State {get; private set;}
    
    public OBD2Engine()
    {
    }
    
    public override void Activate()
    {
        base.Activate();

        if (worker == null){
            worker = new Thread(this.Run);
            worker.Start();
        }
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
        if (Logger.TRACE) Logger.trace("OBD2Engine", "SendCommand:" + command);
        byte[] arr = Encoding.ASCII.GetBytes(command+"\r");
        stream.Write(arr, 0, arr.Length);
    }
    void SendRaw(string command)
    {
        if (Logger.TRACE) Logger.trace("OBD2Engine", "SendRaw:" + command);
        byte[] arr = Encoding.ASCII.GetBytes(command);
        stream.Write(arr, 0, arr.Length);
    }
        
    void SetState(string state2)
    {
        
        State = state2;
        stateTS = DateTime.Now;
        lastReceiveTS = DateTimeMs.Now;
        
        if (Logger.TRACE) Logger.trace("OBD2Engine", " -> " + State);
        
        switch(State){
            case ST_INIT:
                fireStateNotify(STATE_INIT);
                try{
                    stream.Close();
                    Logger.info("OBD2Engine", "Open "+url);
                    stream.Open(url);
                }catch(Exception e){
                    Error = e.Message;
                    Logger.error("OBD2Engine", Error, e);
                    SetState(ST_ERROR);
                    break;
                }
                PurgeStream();

                if (initData != null){
                    extraInitCommands.Clear();
                    extraInitIndex = 0;

                    initData.Split(new char[]{';'}).ToList().ForEach((s) => {
                        var cmd = s.Trim();
                        if (cmd.Length > 0)
                            extraInitCommands.Add(cmd);
                    });
                }
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
            case ST_EXTRAINIT:
                if (extraInitIndex >= extraInitCommands.Count())
                {
                    SetState(ST_SENSOR_INIT);
                }else{
                    SendCommand(extraInitCommands[extraInitIndex]);
                    extraInitIndex++;
                }
                break;
            case ST_SENSOR_INIT:
                SendCommand("0101");
                break;
            case ST_QUERY_PROTOCOL:
                SendCommand("ATDPN");
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
                    
                    // recreate reading timers if layout was changed!
                    if (nextReadings == null || nextReadings.Length != sls.Length){
                        nextReadings = new long[sls.Length];
                    }
                    long nextReading = nextReadings[currentSensorIndex];
                    
                    if (nextReading == 0 || nextReading < DateTimeMs.Now)
                    {
                        if (currentSensorListener.sensor is OBD2Sensor){
                            if (Logger.TRACE) Logger.trace("OBD2Engine", " ----> " + currentSensorListener.sensor.ID);
                            var osensor = (OBD2Sensor)currentSensorListener.sensor;
                            var cmd = osensor.RawCommand;
                            if (cmd != null)
                            {
                                SendCommand(cmd);
                                SetState(ST_SENSOR_ACK);
                                break;
                            }else{
                                // move to next sensor
                            }
                        }
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
                    SetState(ST_EXTRAINIT);
                }
                break;
            case ST_EXTRAINIT:
                SetState(ST_EXTRAINIT);
                break;
            case ST_SENSOR_INIT:
                SetState(ST_QUERY_PROTOCOL);
                break;
            case ST_QUERY_PROTOCOL:
                Protocol = smsg;
                SetState(ST_SENSOR);
                break;
            case ST_SENSOR_ACK:
                
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
                        break;
                    
                    msgraw.Add((byte)((a<<4) + b));
                    
                }
                
                // saving local copy
                var lsl = currentSensorListener;

                var osensor = (OBD2Sensor)lsl.sensor;

                byte[] dataraw = msgraw.ToArray();
                
                nextReadings[currentSensorIndex] = DateTimeMs.Now + lsl.period;
                
                // proactively read next sensor!
                SetState(ST_SENSOR);

                // valid reply - set value, raise listeners
                if (osensor.SetValue(dataraw))
                {
                    subsequentErrors = 0;
                    this.Error = null;
                }else{
                    // search for known errors, increment counters
                	string error = dataErrors.FirstOrDefault(e => smsg.Contains(e));
                	if (error != null)
                	{
                        this.Error = this.Error == null ? error : this.Error + " " + error;
                	    // increase period for this 'bad' sensor
                	    if (subsequentErrors == 0)
                	    {
                            Logger.info("OBD2Engine", "sensor not responding, increasing period: "+osensor.ID);
                	        lsl.period = (lsl.period +100) * 2;
                	    }
                	    subsequentErrors++;
                	}
                }
                // act on too much errors
                if (subsequentErrors > ErrorThreshold) {
                    Logger.error("OBD2Engine", "Connection error threshold");
                    SetState(ST_INIT);
                    subsequentErrors = 0;
                }
                break;
        }
    }
        
    void HandleState()
    {
        
        if (State == ST_ERROR)
        {
            Thread.Sleep(50);
            return;
        }

        // Means no sensor reading was performed - we have
        // to wait and search for another sensor
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
            lastReceiveTS = DateTimeMs.Now;
        }

        // nothing to read -  wait
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

    public int PingTimeout = 1000;
    public int NoResponseTimeout = 5000;
    public int ReconnectTimeout = 10000;
    
    void Run()
    {
        thread_active = true;
        
        SetState(ST_INIT);
        
        while(this.active){
        
            try{
                HandleState();
            }catch(Exception e){
                Logger.error("OBD2Engine", "Run exception", e);
                SetState(ST_ERROR);
            }
            
            // No reply. Ping the connection.
            /*
            if (DateTimeMs.Now - lastReceiveTS > PingTimeout && State != ST_ERROR) {
                Logger.trace("OBD2Engine", "No reply. PING???");
                SendCommand("AT");
                // Only OBDSim bugs??
                SendRaw(" ");
                lastReceiveTS = DateTimeMs.Now;
            }
            */
            // Restart the hanged connection after N seconds
            var diff_ms = DateTimeMs.Now - lastReceiveTS;
            if (diff_ms > NoResponseTimeout) {
                // If ERROR, wait for a longer period before retrying
                if ((this.State != ST_SENSOR_INIT && this.State != ST_ERROR) || diff_ms > ReconnectTimeout)
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
        while(thread_active && counter > 0){
            Thread.Sleep(50);
            counter--;
        }
        // TODO! WTF???
        if (worker != null)
            worker.Abort();
        worker = null;
    }
    
    
    
}

}
