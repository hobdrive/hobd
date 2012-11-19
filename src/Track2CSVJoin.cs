namespace hobd
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Globalization;

	struct Rec{
		public long ts;
		public double value;
	}

    internal class Track2CSVJoin
    {
        private static void Convert(String dir, string[] sensors)
        {
            /*
            var files = from f in Directory.GetFiles(dir, "track0*")
                where !f.Contains(".csv")
                where sensors.FirstOrDefault( s => f.Contains(s)) != null
                select f;
            */

            var dirlist = Directory.GetFiles(dir, "track0*");
            var files = sensors.Select( s => dirlist.FirstOrDefault( df => df.Contains(s) ));

            FileStream stream2 = new FileStream(Path.Combine(dir, "output.csv"), FileMode.Create);
            StreamWriter writer = new StreamWriter(stream2);
            
            var istreams = files.Select( (f) => new FileStream(f, FileMode.Open)).ToList();
            
            var ireaders = istreams.Select( ist => new BinaryReader(ist)).ToList();
            
            var lastd = new Rec[files.Count()];
            var nextd = new Rec[files.Count()];
            
            
            if (UseJS)
            {
                writer.Write("var sensorList = [ ");
                foreach(var s in sensors)
                	writer.Write("'{0}',", s);
                writer.Write("];\n");
            }else
            {
                writer.Write("{0};{1};", "date", "ts");
                foreach(var s in sensors)
                	writer.Write("{0};", s);
                writer.Write("\n");
                foreach(var s in files)
                	writer.Write("{0};", s);
                writer.Write("\n");
            }
            	
            long mints = 0;
            
            if (UseJS)
                writer.Write("var gdata = [");
            
            while(true)
            {
            	
            	/// move the earliest from nextd to lastd
            	var nextmin = nextd.Aggregate((a,b) => a.ts > b.ts ? b : a);
            	var nextmin_i = Array.FindIndex(nextd, r => r.Equals(nextmin));
            	lastd[nextmin_i] = nextmin;
            	lastd[0].ts = nextmin.ts;

                if (mints == 0L)
                {
                    mints = nextmin.ts;
                }
            	
            	/// read new nextd
                try
                {
	            	var newnext = new Rec();
	            	newnext.ts = ireaders.ElementAt(nextmin_i).ReadInt64();
	            	newnext.value = ireaders.ElementAt(nextmin_i).ReadDouble();
	            	nextd.SetValue(newnext, nextmin_i);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Fail: "+ e.ToString());
                }
                
                var ts = lastd[0].ts;
                var value = lastd[0].value;
                string str = new DateTime(ts * 0x2710L).ToString("yyyy MM dd HH:mm:ss.ffff");
                if (UseJS)
                {
                    writer.Write("{2} date: '{0}', ts: {1}, ", str, ts, "{");
                    int si = 0;
                    foreach(var s in lastd)
                    {
                    	writer.Write("{0}: {1}, ", sensors.ElementAt(si), s.value.ToString(DNF));
                    	si++;
                    }
                    writer.Write("},\n");

                }else{
                    writer.Write("{0};{1};", str, ts-mints);
                    foreach(var s in lastd)
                    	writer.Write("{0};", s.value);
                    for(var ui = 0; ui < lastd.Count(); ui++)
                        writer.Write("{0};", ui == nextmin_i ? "*" : "");
                    writer.Write("\n");
                }
            }

            if (UseJS)
                writer.Write("]; // var gdata");
            
            writer.Flush();
            writer.Close();

            foreach(var ird in ireaders)
            	ird.Close();
            foreach(var ist in istreams)
            	ist.Close();
            
            stream2.Close();
        }

        static bool UseJS = false;
    
        public static NumberFormatInfo DNF;


        private static void Main(string[] args)
        {
            DNF = new NumberFormatInfo();
            DNF.NumberDecimalSeparator = ".";
            DNF.PositiveInfinitySymbol = "∞";
            DNF.NaNSymbol = "-";

            if (args.Length < 2)
            {
                Console.WriteLine("Use: Track2CSVJoin [-js] <path to track dir> Sensor1 Sensor2 Sensor3 ...");
                return;
            }
            if (args[0] == "-js")
            {
                args = args.Skip(1).ToArray();
                UseJS = true;
            }

            var sensors = args.Skip(1);
            Convert(args[0], sensors.ToArray());
        }
    }
}

