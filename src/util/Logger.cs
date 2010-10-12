using System;
using System.IO;

namespace hobd{

public class Logger
{
    public const bool TRACE = true;
    
    static StreamWriter fs = new StreamWriter(new FileStream( Path.Combine(HOBD.AppPath, "log.txt"), FileMode.Append));
    
    public static void trace(String s)
    {
        if (TRACE) log("TRACE", s);
    }
    
    static void log(string level, string s)
    {
        s = "["+level+"] "+DateTime.Now.ToLongTimeString() + ":    " + s;
        System.Console.WriteLine(s);
        fs.Write(s+"\n");
        fs.Flush();    
    }

}


}