using System;
using System.IO;

namespace hobd{

public class Logger
{
    public const bool TRACE = true;
    public const bool INFO = true;
    public const bool ERROR = true;
    
    static StreamWriter fs = new StreamWriter(new FileStream( Path.Combine(HOBD.AppPath, "log.txt"), FileMode.Append));
    
    public static void error(String comp, String msg, Exception e)
    {
        if (ERROR) log("ERROR", comp, msg, e);
    }

    public static void info(String comp, String msg)
    {
        if (INFO) log("INFO", comp, msg, null);
    }

    public static void trace(String comp, String msg)
    {
        if (TRACE) log("TRACE", comp, msg, null);
    }

    public static void trace(String msg)
    {
        if (TRACE) log("TRACE", "", msg, null);
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