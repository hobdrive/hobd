namespace hobd
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;

    class TrackRecovery
    {
        public const int SKIP = 15*60*1000;
        //public const double VALUE_THRES = 100000;

        public static Dictionary<string, double> collects = new Dictionary<string, double>();
        public static Dictionary<string, double[]> fcollects = new Dictionary<string, double[]>();


        static bool SameDayFilter(long csTimeStamp, long thatTimeStamp)
        {
            if (DateTimeMs.ToDateTime(csTimeStamp).Date != DateTimeMs.ToDateTime(thatTimeStamp).Date )
                return true;
            else
                return false;
        }

        static bool SameWeekFilter(long csTimeStamp, long thatTimeStamp)
        {
            var d1 = DateTimeMs.ToDateTime(csTimeStamp);
            var d2 = DateTimeMs.ToDateTime(thatTimeStamp);
            var jan1 = (int)new DateTime(d1.Year, 1, 1, 0,0,0).DayOfWeek;
            int wk1 = (d1.DayOfYear + jan1 - 2) / 7;
            int wk2 = (d2.DayOfYear + jan1 - 2) / 7;
            return (wk1 != wk2);
        }
        
        static bool SameMonthFilter(long csTimeStamp, long thatTimeStamp)
        {
            var d1 = DateTimeMs.ToDateTime(csTimeStamp).Month;
            var d2 = DateTimeMs.ToDateTime(thatTimeStamp).Month;
            return (d1 != d2);
        }


        private static string Collect(string file)
        {
            var sname = Path.GetFileName(file).Replace("track0.", "");

            if ( (new String[]{ "maximum","_fueling","_fueling","_tank","Efficiency" }).FirstOrDefault( e => sname.Contains(e) ) != null)
                return null;

            if ( sname.Contains("Filtered") )
                return null;

            double value = 0;
            if (collects.ContainsKey(sname))
                value = collects[sname];
            
            double vDay, vWeek, vMonth;
            vDay = vWeek = vMonth = 0;

            using (FileStream stream = new FileStream(file, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(stream);

                long   pTS = 0L;
                double pValue = 0;
                double averageIncrease = 0;
                int averageIncreaseCount = 1;

                bool restart = true;
                
                while(true)
                {
                    try
                    {
                        long cTS = reader.ReadInt64();
                        double cValue = reader.ReadDouble();
                        string str = new DateTime(cTS * 10000).ToString("yyyy MM dd HH:mm:ss.ffff");

                        if (cTS == 0 || cTS == unchecked((long)0xFFFFFFFFFFFFFFFF))
                            continue;

                        if (!restart)
                        {
                            if (cTS-pTS > SKIP)
                                restart = true;
                            if (cValue-pValue < 0 || (averageIncreaseCount > 10 && (cValue-pValue > averageIncrease*(cTS-pTS)*100)))
                            {
                                System.Console.WriteLine("RESTART " + (double)(cValue - pValue) + " " + str + " - " + averageIncrease*100);
                                restart = true;
                            }
                        }

                        if (restart)
                        {
                            pTS = cTS;
                            pValue = cValue;
                            restart = false;
                        }
                        
                        var diff = (cValue - pValue);
                        value += diff;

                        if (!SameDayFilter(DateTimeMs.Now, cTS) && vDay == 0)
                            vDay = value;
                        if (!SameWeekFilter(DateTimeMs.Now, cTS) && vWeek == 0)
                            vWeek = value;
                        if (!SameMonthFilter(DateTimeMs.Now, cTS) && vMonth == 0)
                            vMonth = value;

                        if (cTS > pTS)
                        {
                            averageIncrease = (averageIncrease*averageIncreaseCount + diff/(cTS-pTS))/(averageIncreaseCount+1);
                            averageIncreaseCount++;
                        }
                        
                        pTS = cTS;
                        pValue = cValue;
                    }
                    catch (EndOfStreamException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine("Fail: ", e.Message);
                    }
                }
                reader.Close();
            }
            collects[sname] = value;
            fcollects[sname] = new[]{ vDay, vWeek, vMonth };
            return sname;
        }

        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Use: Track2CSV <path to track dir>");
                return;
            }
            foreach(var dir in args)
                RecoverDir(dir);
            foreach(var k in collects.Keys)
                System.Console.WriteLine(k +" "+collects[k]);
            foreach(var k in fcollects.Keys)
                System.Console.WriteLine("Filtered."+k +" day="+fcollects[k][0]+" week="+fcollects[k][1] + " month="+fcollects[k][2]);
        }

        public static void RecoverDir(string dir)
        {
            (from f in Directory.GetFiles(dir, "track0*") where !f.Contains("csv") select f)
              .ToList<string>().ForEach(delegate (string f)
            {
                try
                {
                    var sname = Collect(f);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Failure: " + f + " " + exception.Message);
                }
            });
        }
    }
}

