namespace hobd
{
    using System;
    using System.IO;
    using System.Linq;

    internal class Track2CSV
    {
        private static void Convert(string file)
        {
            using (FileStream stream = new FileStream(file, FileMode.Open))
            {
                bool flag;
                BinaryReader reader = new BinaryReader(stream);
                FileStream stream2 = new FileStream(file + ".csv", FileMode.Create);
                StreamWriter writer = new StreamWriter(stream2);
                long num = 0L;
                goto Label_009F;
            Label_0030:;
                try
                {
                    long num2 = reader.ReadInt64();
                    double num3 = reader.ReadDouble();
                    string str = new DateTime(num2 * 0x2710L).ToString("yyyy MM d HH:mm:ss.ffff");
                    if (num == 0L)
                    {
                        num = num2;
                    }
                    long num4 = num2 - num;
                    writer.WriteLine("{0};{1};{2}", str, num4, num3);
                }
                catch (EndOfStreamException)
                {
                    goto Label_00A4;
                }
            Label_009F:
                flag = true;
                goto Label_0030;
            Label_00A4:
                reader.Close();
                writer.Close();
                stream2.Close();
            }
        }

        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Use: Track2CSV <path to track dir>");
            }
            else
            {
                (from f in Directory.GetFiles(args[0], "track0*")
                    where !f.Contains(".csv")
                    select f).ToList<string>().ForEach(delegate (string f) {
                    try
                    {
                        Convert(f);
                        Console.WriteLine("Success: " + f);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("Failure: " + f + " " + exception.Message);
                    }
                });
            }
        }
    }
}

