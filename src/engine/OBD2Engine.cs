using System;
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
    public const string ST_ATZ_ACK = "ATZ_ACK";
        /*
        ATE,
        ATE_ACK,
        SENSOR,
        SENSOR_ACK
        */

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
        while(stream.HasData())
            stream.Read();
    }
    
    void SendCommand(string command)
    {
        byte[] arr = Encoding.ASCII.GetBytes(command);
        stream.Write(arr, 0, arr.Length);
    }
    
    void SetState(string state2)
    {
        State = state2;
        stateTS = DateTime.Now;
        
        Logger.trace("OBD2Engine.SetState " + State);
        
        switch(State){
            case ST_INIT:
                stream.Close();
                stream.Open(url);
                PurgeStream();
                SetState(ST_ATZ);
                break;
            case ST_ATZ:
                SendCommand("ATZ");
                break;
        }
    }
    
    byte[] buffer = new byte[256];
    int position = 0;
    
    void HandleState()
    {
        if (!stream.HasData()) return;
        
        byte[] data = stream.Read();
        Array.Copy(data, 0, buffer, position, data.Length);

        foreach(SensorListener sl in Registry.ActiveSensors)
        {
            System.Console.WriteLine(""+sl.sensor.ID);
            if (sl.sensor is OBD2Sensor){
                ((OBD2Sensor)sl.sensor).v = new Random().NextDouble() *100;
            }
            foreach(Action<Sensor> l in sl.listeners){
                l(sl.sensor);
            }
        }
    }
    
    void Run()
    {
        thread_active = true;
        
        SetState(ST_INIT);
        
        while(this.active){
        
            HandleState();

            if (DateTime.Now.Subtract(stateTS).TotalSeconds > 2000) {
                // Restart the hanged connection
                SetState(ST_INIT);
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
