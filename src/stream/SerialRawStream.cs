using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO.Ports;

namespace hobd
{

/*
 * This is the simple serial stream with the only difference is that it uses BytesToRead to know if data are available
 */
public class SerialRawStream: IStream
{
    SerialPort port;

    public SerialRawStream()
    {}

    /**
     * returns string array of {address, serviceid, pin}
     */
    public static string[] ParseUrl(string url)
    {
        Regex rx = new Regex(@"^([^;]+) (\;baud=(\d+))? (\;parity=(none|odd|even|mark|space))? (\;handshake=(none|x|rts|xrts))? $", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        
        var match = rx.Match(url);
        
        if (match == null)
            return new string[]{null, null, null, null};
        
        var port = match.Groups[1].Value;
        if (port == "") port = null;

        var baud = match.Groups[3].Value;
        if (baud == "") baud = null;

        var parity = match.Groups[5].Value;
        if (parity == "") parity = null;

        var hs = match.Groups[7].Value;
        if (hs == "") hs = null;

        return new string[]{ port, baud, parity, hs };
    }
    public const int URL_PORT   = 0;
    public const int URL_BAUD   = 1;
    public const int URL_PARITY = 2;
    public const int URL_HANDSHAKE = 3;

    public void Open(String url)
    {
        var u = SerialStream.ParseUrl(url);

        Logger.trace("SerialStream", "Port "+u[0]+" baud "+u[1]+" parity "+u[2]+" hs "+u[3]);

        var baudRate = 38400;
        var parity = Parity.None;
        var dataBits = 8;
        var stopBits = StopBits.One;

        if (u[URL_BAUD] != null)
            baudRate = int.Parse(u[URL_BAUD]);
        switch (u[URL_PARITY]){
            case "none":
            parity = Parity.None;
            break;
            case "odd":
            parity = Parity.Odd;
            break;
            case "even":
            parity = Parity.Even;
            break;
            case "mark":
            parity = Parity.Mark;
            break;
            case "space":
            parity = Parity.Space;
            break;
        }
        
        port = new SerialPort(u[URL_PORT], baudRate, parity, dataBits, stopBits);

        switch (u[URL_HANDSHAKE]){
            case "none":
            port.Handshake = Handshake.None;
            break;
            case "x":
            port.Handshake = Handshake.XOnXOff;
            break;
            case "rts":
            port.Handshake = Handshake.RequestToSend;
            break;
            case "xrts":
            port.Handshake = Handshake.RequestToSendXOnXOff;
            break;
        }
        
        try{
            port.ReadBufferSize = 0x40;
            port.ReceivedBytesThreshold = 1;
            port.ReadTimeout = 2000;
            port.WriteTimeout = 2000;
        }catch(Exception){}
        try{
            port.Open();
        }catch(Exception e){
            port = null;
            throw e;
        }
    }
    
    public void Close()
    {
        try{
            if (port != null)
                port.Close();
        }catch(Exception){}
        port = null;
    }
    
    public bool HasData()
    {
        if (port == null)
            return false;
        try{
            return port.BytesToRead != 0;
        }catch(Exception){
            return false;
        }
    }
    
    byte[] sbuf = new byte[128];
    public byte[] Read()
    {
        if(port == null) return null;
        
        int read;
        try{
            read = port.Read(sbuf, 0, 128);
        }catch(Exception ex){
            Close();
            Logger.error("SerialPort", "port.Read failed", ex);
            return null;
        }
        byte[] buf = new byte[read];
        Array.Copy(sbuf, buf, read);
        return buf;
    }
    
    public void Write(byte[] array, int offset, int length)
    {
        try{
            if(port != null)
                port.Write(array, offset, length);
        }catch(Exception){
            Close();
            return;
        }
    }
}

}