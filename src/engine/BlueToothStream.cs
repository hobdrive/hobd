using System;
using System.Text;
using System.Collections;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Ports;

namespace hobd
{

public class BluetoothStream: IStream
{
    NetworkStream stream;
    BluetoothClient bluetoothClient;

    public BluetoothStream()
    {}

    /**
     * returns string array of {address, serviceid, pin}
     */
    public static string[] ParseUrl(string url)
    {
        Regex rx = new Regex(@"^btspp:// ([\d\w]+) (\:(\d+))? (\;pin=(\d+))? $", RegexOptions.IgnorePatternWhitespace);
        
        var match = rx.Match(url);
        
        if (match == null)
            return new string[]{null, null, null};
        
        var serviceid = match.Groups[3].Value;
        if (serviceid == "") serviceid = null;

        var pid = match.Groups[5].Value;
        if (pid == "") pid = null;

        return new string[]{ match.Groups[1].Value, serviceid, pid };
    }
    
    public void Open(String url)
    {
        try{
            
            var parsed_url = ParseUrl(url);
            Logger.trace("BluetoothStream", "Open " + parsed_url[0] + " serviceid " + parsed_url[1] + " pin " + parsed_url[2]);

            BluetoothRadio.PrimaryRadio.Mode = RadioMode.Discoverable;

            BluetoothAddress address = BluetoothAddress.Parse(parsed_url[0]);

            bluetoothClient = new BluetoothClient();
            if (parsed_url[2] != null)
                bluetoothClient.SetPin(address, parsed_url[2]);
            BluetoothEndPoint btep;
            if (parsed_url[1] != null)
                btep = new BluetoothEndPoint(address, BluetoothService.SerialPort, int.Parse(parsed_url[1]));
            else
                btep = new BluetoothEndPoint(address, BluetoothService.SerialPort);
            bluetoothClient.Connect(btep);
            stream = bluetoothClient.GetStream();
            if (stream == null){
                bluetoothClient.Close();
                bluetoothClient = null;
            }else{
                if (stream.CanTimeout){
                    stream.WriteTimeout = 2;
                    stream.ReadTimeout = 2;
                }
            }
        }catch(Exception e){
            if (bluetoothClient != null)
            {
                bluetoothClient.Close();
                bluetoothClient = null;
            }
            throw e;
        }
    }
    
    public void Close()
    {
        if (bluetoothClient != null)
        {
            bluetoothClient.Close();
            bluetoothClient = null;
            stream.Close();
        }
}
    
    public bool HasData()
    {
        if (bluetoothClient != null && stream != null)
        {
            try{
                return stream.DataAvailable;
            }catch(Exception){
                bluetoothClient = null;
                return false;
            }
        }
            return false;
    }
    
    public byte[] Read()
    {
        if (bluetoothClient != null)
        {
            byte[] buf = new byte[1024];
            int len;
            try{
                len = stream.Read(buf, 0, buf.Length);
            }catch(System.IO.IOException){
                bluetoothClient = null;
                return null;
            }
            byte[] outputData_ = new byte[len];
            Array.Copy(buf, 0, outputData_, 0, len);
            return outputData_;
        }
        return null;
    }
    
    public void Write(byte[] array, int offset, int length)
    {
        if (bluetoothClient != null && stream != null)
        {
            try{
                stream.Write(array, offset, length);
            }catch(Exception){
                bluetoothClient = null;
            }
        }
    }
}
}