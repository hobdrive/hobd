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

        private static string Collect(string file)
        {
            var sname = Path.GetFileName(file).Replace("track0.", "");

            double value = 0;
            if (collects.ContainsKey(sname))
                value = collects[sname];

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
                                System.Console.WriteLine("JUMP " + (double)(cValue - pValue) + " " + str + " - " + averageIncrease*100);
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

