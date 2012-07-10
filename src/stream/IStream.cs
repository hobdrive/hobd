namespace hobd
{
using System;

/// <summary>
/// Abstract stream, which is used by Engines (f.e. by OBD2Engine) to interact with ECU 
/// </summary>
public interface IStream
{
    void Open(String url);

    void Close();
    
    bool HasData();
    
    byte[] Read();
    
    void Write(byte[] array, int offset, int length);
}

}