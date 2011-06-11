using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace hobd{

public class DTCTools
{

    /**
     *  Searches the files in "fileMask" for DTC descriptions in form:
     *  P0000 Description
     */
    public static Dictionary<string, List<string>> Decode(string filePath, string fileMask, IEnumerable<string> dtcs)
    {
        var result = new Dictionary<string, List<string>>();

        try{
            Directory.GetFiles(filePath, fileMask).ToList().ForEach( (f) => {
                var sr = new StreamReader(new FileStream(f, FileMode.Open));
                while(true)
                {
                    var line = sr.ReadLine();
                    if (line == null)
                        break;
                    if (line.Length < 6 || line[5] != ' ')
                        continue;
                    
                    var cur_dtc = line.Substring(0, 5);
                    if ( dtcs.FirstOrDefault((dtc) => dtc == cur_dtc) != null)
                    {
                        if (!result.ContainsKey(cur_dtc))
                            result.Add(cur_dtc, new List<string>());
                        result[cur_dtc].Add(line.Substring(6).Trim());
                    }
                }
                sr.Close();
            });
        }catch(Exception e){
            Logger.error("DTCTools", "failed", e);
        }

        return result;
    }

}

}