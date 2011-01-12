/*
 * fck all the ms shit!
 * patch to get MS from opennetcf
*/
using System;

namespace hobd{

public class DateTimeMs
{
    private static int m_offset = 0;
    private static long systemStartMS = 0;
     
    static DateTimeMs()
    {
       int s = DateTime.Now.Second;
       while (true)
       {
         int s2 = DateTime.Now.Second;
     
         // wait for a rollover
         if (s != s2)
         {
           m_offset = Environment.TickCount % 1000;
           break;
         }
       }
       systemStartMS = (DateTime.Now - new TimeSpan((long)(Environment.TickCount)*10000)).Ticks / 10000;
    }

    public static DateTime NowMs
    {
       get
       {
         // find where we are based on the os tick
         int tick = Environment.TickCount % 1000;
     
         // calculate our ms shift from our base m_offset
         int ms = (tick >= m_offset) ? (tick - m_offset) : (1000 - (m_offset - tick));
     
         // build a new DateTime with our calculated ms
         // we use a new DateTime because some devices fill ms with a non-zero garbage value
         DateTime now = DateTime.Now;
         return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, ms);
       }
    }

    public static DateTime ToDateTime(long ts)
    {
        return new DateTime(ts * 10000);
    }

    /**
     * Number of milliseconds from 01.01.0001
     */
    public static long Now
    {
       get
       {
         //return systemStartMS + Environment.TickCount;
         return NowMs.Ticks / 10000;
       }
    }

}
}