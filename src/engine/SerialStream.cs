using System;
using System.Text;
using System.Collections;
using System.IO.Ports;

namespace hobd
{

public class SerialStream: IStream
{
    SerialPort port;
    Queue portQueue = new Queue();

    SerialStream()
    {}

    public void Open(String url)
    {
        port = new SerialPort(url);

        port.BaudRate = 9600;
        port.Parity = Parity.None;
        port.StopBits = StopBits.One;
        port.DataBits = 8;
        port.Handshake = Handshake.None;
        port.ReadTimeout = 2000;
        port.WriteTimeout = 2000;

        port.DataReceived += new SerialDataReceivedEventHandler(DataReceviedHandler);

        try {
            port.Open();
        }catch(Exception e){
            port = null;
        }
    }
    
    private void DataReceviedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort port = (SerialPort)sender;
        byte[] buf = new byte[128];
        int read = port.Read(buf, 0, 128 );
        portQueue.Enqueue(buf);

        Logger.trace("Data Received:"+read);
    }

    public void Close()
    {
        if (port != null)
            port.Close();
    }
    
    public bool HasData()
    {
        if (port == null) return false;
        return portQueue.Count > 0;
    }
    
    public byte[] Read()
    {
        return (byte[])portQueue.Dequeue();
    }
    
    public void Write(byte[] array, int offset, int length)
    {
        if(port != null)
            port.Write(array, offset, length);
    }
}

}