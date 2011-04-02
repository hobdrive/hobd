using System;
using System.Collections.Generic;
using System.Globalization;

namespace hobd{

/**
 * Converts from metric <=> imperial units
 */
public class UnitsConverter{

    public string Units{get; private set;}

    static Dictionary<string, Func<double,double>> metric = new Dictionary<string, Func<double,double>>();
    static Dictionary<string, Func<double,double>> imperial = new Dictionary<string, Func<double,double>>();
    static Dictionary<string, string> metric_s = new Dictionary<string, string>();
    static Dictionary<string, string> imperial_s = new Dictionary<string, string>();

    public static NumberFormatInfo DefaultNumberFormat;

    static UnitsConverter(){

        DefaultNumberFormat = new NumberFormatInfo();
        DefaultNumberFormat.NumberDecimalSeparator = ".";
        DefaultNumberFormat.PositiveInfinitySymbol = "∞";
        
        var i = new Dictionary<string, Func<double, double>>();
        var i_s = new Dictionary<string, string>();
        var m = new Dictionary<string, Func<double, double>>();
        var m_s = new Dictionary<string, string>();

        i.Add("celsius",    (v) => v*9/5 + 32);
        m.Add("fahrenheit", (v) => (v-32)*5/9);
        i_s.Add("celsius", "fahrenheit");
        m_s.Add("fahrenheit", "celsius");

        i.Add("kph", (v) => v/1.61);
        m.Add("mph", (v) => v*1.61);
        i_s.Add("kph", "mph");
        m_s.Add("mph", "kph");
        
        i.Add("lph", (v) => 100 / (v*0.425));
        m.Add("mpg", (v) => 100 / (v*0.425));
        i_s.Add("lph", "mpg");
        m_s.Add("mpg", "lph");

        i.Add("lphour", (v) => v / 3.785);
        m.Add("ghour",  (v) => v*3.785);
        i_s.Add("lphour", "ghour");
        m_s.Add("ghour", "lphour");

        i.Add("km", (v) => v/1.61);
        m.Add("miles", (v) => v*1.61);
        i_s.Add("km", "miles");
        m_s.Add("miles", "km");

        i.Add("liters", (v) => v / 3.785);
        m.Add("gallons", (v) => v*3.785);
        i_s.Add("liters", "gallons");
        m_s.Add("gallons", "liters");

        UnitsConverter.metric = m;
        UnitsConverter.imperial = i;
        UnitsConverter.metric_s = m_s;
        UnitsConverter.imperial_s = i_s;
    }

    public UnitsConverter(string units)
    {
       this.Units = units;
    }

    public bool NeedConversion(string fromUnits)
    {
        if (fromUnits == null) return false;

        if (this.Units == "metric"){
            return metric.ContainsKey(fromUnits);
        }else{
            return imperial.ContainsKey(fromUnits);
        }
    }

    public double Convert(string fromUnits, double value)
    {
        if (this.Units == "metric"){
            if (!metric.ContainsKey(fromUnits))
                return value;
            else
                return metric[fromUnits](value);
        }else{
            if (!imperial.ContainsKey(fromUnits))
                return value;
            else
                return imperial[fromUnits](value);
        }
    }

    public string ConvertUnits(string fromUnits)
    {
        if (this.Units == "metric"){
            if (!metric_s.ContainsKey(fromUnits))
                return fromUnits;
            else
                return metric_s[fromUnits];
        }else{
            if (!imperial_s.ContainsKey(fromUnits))
                return fromUnits;
            else
                return imperial_s[fromUnits];
        }
    }

}
}