using System;
using System.IO;

namespace hobd{

public class Logger
{
    public const bool TRACE = true;
    public const bool ERROR = true;
    
    static StreamWriter fs = new StreamWriter(new FileStream( Path.Combine(HOBD.AppPath, "log.txt"), FileMode.Append));
    
    public static void error(String comp, String msg, Exception e)
    {
        if (ERROR) log("ERROR", comp, msg, e);
    }

    public static void trace(String s)
    {
        if (TRACE) log("TRACE", "", s, null);
    }
    
    static void log(string level, string comp, string msg, Exception e)
    {
        if (comp == null) comp = "";
        if (msg == null) msg = "";
        
        msg = "["+level+"] ["+comp+"] "+DateTime.Now.ToLongTimeString() + ":    " + msg;
        if (e != null)
        {
            msg +=  " " + e.Message + e.StackTrace;
        }
        System.Console.WriteLine(msg);
        fs.Write(msg+"\n");
        fs.Flush();    
    }

}


}