using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace hobd
{

public class TCPStream: IStream
{
    Queue portQueue = new Queue();
    Socket sock;

    public TCPStream()
    {}

    /**
     * returns string array of { address, port }
     */
    public static string[] ParseUrl(string url)
    {
        Regex rx = new Regex(@"^tcp:// ([\d\w\.\-]+) \: (\d+)? $", RegexOptions.IgnorePatternWhitespace);
        
        var match = rx.Match(url);
        
        if (match == null)
            return new string[]{null, null};

        return new string[]{ match.Groups[1].Value, match.Groups[2].Value };
    }

    public const int URL_ADDR   = 0;
    public const int URL_PORT   = 1;    
    
    /**
      URL is in form of:
      tcp://192.168.1.2:200
    */
    public void Open(String url)
    {
        var u = TCPStream.ParseUrl(url);
			
		string host = u[URL_ADDR];
		int port = int.Parse(u[URL_PORT]);

        Logger.trace("TCPStream", "TCP Stream: "+ host + ":"+port);

        try
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(new IPEndPoint(IPAddress.Parse(host), port));
        }
        catch (Exception){}
        if (sock != null && sock.Connected)
        {
            //sock.ReceiveTimeout = 50;
            //sock.ReceiveBufferSize = 128;
        }else{
            sock = null;
            throw new Exception("Can't connect socket " + url);
        }
    }
    
    public void Close()
    {
        if (sock != null)
            sock.Close();
    }
    
    public bool HasData()
    {
        if (sock == null) return false;
        try
        {
            return sock.Poll(0, SelectMode.SelectRead);
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    byte[] buf = new byte[128];
    public byte[] Read()
    {
        lock(this)
        {
            var len = sock.Receive(buf);
            if (len == 0)
                return null;
            byte[] outputData_ = new byte[len];
            Array.Copy(buf, 0, outputData_, 0, len);
            return outputData_;
        }
    }
    
    public void Write(byte[] array, int offset, int length)
    {
        try{
            if (sock != null)
            {
                sock.Send(array, offset, length, SocketFlags.None);
            }
        }catch(Exception){}
    }
}

}