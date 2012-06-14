using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace hobd{

/**
 * Converts from metric <=> imperial units
 */
public class UnitsConverter{

    public string Units{get; private set;}
    Dictionary<string, Func<double,double>> CurrentConversions = new Dictionary<string, Func<double,double>>();

    /**
      Each entry is:
      "km-miles" (v) => v/1.61
      "miles-km" (v) => v*1.61
     */
    static Dictionary<string, Func<double,double>> conversions = new Dictionary<string, Func<double,double>>();

    static Dictionary<string, string> UnitTypes = new Dictionary<string, string>();
    static Dictionary<string, List<string>> UnitTypesRaw = new Dictionary<string, List<string>>();

    public static NumberFormatInfo DefaultNumberFormat;

    static UnitsConverter(){

        DefaultNumberFormat = new NumberFormatInfo();
        DefaultNumberFormat.NumberDecimalSeparator = ".";
        DefaultNumberFormat.PositiveInfinitySymbol = "∞";
        DefaultNumberFormat.NaNSymbol = "-";
        
        conversions.Add("fahrenheit-celsius",    (v) => (v-32)*5/9);
        conversions.Add("celsius-fahrenheit", (v) => v*9/5 + 32);

        conversions.Add("km-miles", (v) => v/1.609);
        conversions.Add("miles-km", (v) => v*1.609);

        conversions.Add("mph-kph", conversions["miles-km"]);
        conversions.Add("kph-mph", conversions["km-miles"]);

        conversions.Add("kw-hp", (v) => v/0.745);
        conversions.Add("hp-kw", (v) => v*0.745);

        conversions.Add("Nm-lbft", (v) => v*0.7375621);
        conversions.Add("lbft-Nm", (v) => v/0.7375621);

        // volume
        conversions.Add("liters-gallons", (v) => v / 3.785);
        conversions.Add("gallons-liters", (v) => v*3.785);

        conversions.Add("liters-ukgallons", (v) => v / 4.546);
        conversions.Add("ukgallons-liters", (v) => v * 4.546);

        // FE
        // km per liter
        conversions.Add("lph-kmpl", (v) => 100 / v);
        conversions.Add("kmpl-lph", (v) => 100 / v);

        conversions.Add("mpg-lph", (v) => 100 / (v*1.609/3.785));
        conversions.Add("lph-mpg", (v) => 100 / (v*1.609/3.785));

        conversions.Add("ukmpg-lph", (v) => 100 / (v*1.609/4.546));
        conversions.Add("lph-ukmpg", (v) => 100 / (v*1.609/4.546));

        // FE instant
        conversions.Add("ghour-lphour", conversions["gallons-liters"]);
        conversions.Add("lphour-ghour",  conversions["liters-gallons"]);

        conversions.Add("ukghour-lphour", conversions["ukgallons-liters"]);
        conversions.Add("lphour-ukghour",  conversions["liters-ukgallons"]);

        UnitTypes["metric"]   = "celsius km kph lph liters lphour hp Nm";
        UnitTypes["imperial"] = "fahrenheit miles mph mpg gallons ghour hp lbft";
        UnitTypes["uk"]       = "celsius miles mph ukmpg ukgallons ukghour kw lbft";
        UnitTypes["africa"]   = "celsius km kph kmpl liters lphour kw Nm";

        UpdateUnitTypes();

    }

    public static void UpdateUnitTypes()
    {
        foreach(var u in UnitTypes.Keys){
            UnitTypesRaw[u] = UnitTypes[u].Split(new char[]{' '}).ToList();
        }
    }

    public static IEnumerable<string> AvailableTypes{
        get{
            return UnitTypes.Keys;
        }
    }

    public UnitsConverter(string units)
    {
       this.Units = units;
       
       foreach(var u in UnitTypesRaw[Units]){
           foreach(var u2 in conversions.Keys.Where((k) => k.EndsWith("-"+u))){
               var fromu = u2.Substring(0, u2.IndexOf("-"));
               CurrentConversions[fromu] = conversions[u2];
           }
       }
    }

    public bool NeedConversion(string fromUnits)
    {
        if (fromUnits == null) return false;

        return CurrentConversions.ContainsKey(fromUnits);
    }
    /*
    public static Func<double, double> SearchMapping(string unitsType, string fromUnits)
    {
        var myUnits = UnitsTypesRaw(unitsType);
        var toUnits = myUnits.FirstOrDefault((u) => conversions.ContainsKey(fromUnits+"-"+u));
        
        return conversions.Keys.FirstOrDefault((k) => k == fromUnits);
    }
    */
    public double Convert(string fromUnits, double value)
    {
        if (fromUnits == null)
            return value;
        if (!NeedConversion(fromUnits))
            return value;
        return CurrentConversions[fromUnits](value);
    }

    public static double Convert(string fromUnits, string toUnits, double value)
    {
        if (fromUnits == null || toUnits == null)
            return value;
        
        if (!conversions.ContainsKey(fromUnits+"-"+toUnits))
            return value;

        return conversions[fromUnits+"-"+toUnits](value);
    }
    
    public string ConvertUnits(string fromUnits)
    {
        if (fromUnits == null)
            return null;
        if (UnitTypesRaw[Units].Contains(fromUnits))
            return fromUnits;
        var result = conversions.Keys
              .Where((k) => k.StartsWith(fromUnits+"-"))
              .Select( (u2) => u2.Substring(u2.IndexOf("-")+1) )
              .Where( (u2) => UnitTypesRaw[Units].Contains(u2)  ).FirstOrDefault();
       return result == null ? fromUnits : result;
    }

}
}