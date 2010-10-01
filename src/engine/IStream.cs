namespace hobd
{
using System;

public interface IStream
{
    void Open(String url);

    void Close();
    
    bool HasData();
    
    byte[] Read();
    
    void Write(byte[] array, int offset, int length);
}

}