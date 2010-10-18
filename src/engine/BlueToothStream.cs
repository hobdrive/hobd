using System;
using System.Text;
using System.Collections;
using System.Net.Sockets;
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

    public void Open(String url)
    {
		try{
			BluetoothAddress address = BluetoothAddress.Parse(url);
			bluetoothClient = new BluetoothClient();
			bluetoothClient.SetPin(address, "0000");
			BluetoothEndPoint btep = new BluetoothEndPoint(address, BluetoothService.SerialPort, 1);
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
		}catch(System.IO.IOException){
			bluetoothClient = null;
		}
    }
    
    public void Close()
    {
		if (bluetoothClient != null)
		{
			bluetoothClient.Close();
			bluetoothClient = null;
		}
}
    
    public bool HasData()
    {
		if (bluetoothClient != null && stream != null)
		{
			try{
				return stream.DataAvailable;
			}catch(System.IO.IOException){
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
			}catch(System.IO.IOException){
				bluetoothClient = null;
			}
		}
    }
}
}