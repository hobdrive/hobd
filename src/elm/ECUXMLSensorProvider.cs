using System;
using System.IO;
using System.Xml;
using System.Globalization;

namespace hobd{

public class ECUXMLSensorProvider : SensorProvider
{

    public string Namespace{get; set;}
    public string Description{get; set;}

    XmlReader reader;

    public ECUXMLSensorProvider(string ecuxml)
    {
        XmlReaderSettings xrs = new XmlReaderSettings();
        xrs.IgnoreWhitespace = true;
        xrs.IgnoreComments = true;
        reader = XmlReader.Create(ecuxml, xrs);
    }
    /*
    public ECUXMLSensorProvider(File ecuxml)
    {
        XmlReaderSettings xrs = new XmlReaderSettings();
        xrs.IgnoreWhitespace = true;
        xrs.IgnoreComments = true;
        reader = XmlReader.Create(file, xrs);
    }
    */
    void init(SensorRegistry registry)
    {
        if (reader == null)
            throw new Exception("Can't do double init");

        reader.Read();

        this.Namespace = reader.GetAttribute("namespace") ?? "Default";
        this.Description = reader.GetAttribute("description") ?? "";

        reader.ReadStartElement("parameters");

        while( reader.IsStartElement("parameter") ){

            var id = reader.GetAttribute("id");
            var display = reader.GetAttribute("display");

            reader.ReadStartElement();

            int command = 0;
            string rawcommand = null;
            string basename = null;
            string units = null;
            string valuea = null;
            string valueb = null;
            string valuec = null;
            string valued = null;
            string valueab = null;
            string valuebc = null;
            string valuecd = null;
            double offset = 0;

            while(reader.NodeType == XmlNodeType.Element)
            {
                switch(reader.Name)
                {
                    case "address":
                        reader.ReadStartElement();
                        var hexval = reader.ReadElementString("byte").Trim();
                        if (hexval.StartsWith("0x"))
                            hexval = hexval.Substring(2);
                        command = int.Parse(hexval, NumberStyles.HexNumber);
                        reader.ReadEndElement();
                        break;
                    case "raw":
                        rawcommand = reader.ReadElementString().Trim();
                        break;
                    case "base":
                        basename = reader.ReadElementString().Trim();
                        break;

                    case "valuea":
                        valuea = reader.ReadElementString();
                        break;
                    case "valueb":
                        valueb = reader.ReadElementString();
                        break;
                    case "valuec":
                        valuec = reader.ReadElementString();
                        break;
                    case "valued":
                        valued = reader.ReadElementString();
                        break;

                    case "valueab":
                        valueab = reader.ReadElementString();
                        break;
                    case "valuebc":
                        valuebc = reader.ReadElementString();
                        break;
                    case "valuecd":
                        valuecd = reader.ReadElementString();
                        break;

                    case "offset":
                        offset = double.Parse(reader.ReadElementString(), UnitsConverter.DefaultNumberFormat);
                        break;
                    case "description":
                        reader.ReadStartElement();
                        while(reader.NodeType == XmlNodeType.Element)
                        {
                            switch(reader.Name)
                            {
                                case "unit":
                                    units = reader.ReadElementString().Trim();
                                    break;
                                default:
                                    reader.ReadElementString();
                                    break;
                            }
                        }
                        reader.ReadEndElement();
                        break;
                    default:
                        throw new Exception("unknown tag `"+reader.Name+"` while creating PID "+id);
                }
            }

            CoreSensor sensor = null;
            // OBD2 sensor
            if (basename == null)
            {
                var s = new OBD2Sensor();
                if (rawcommand != null)
                    s.RawCommand = rawcommand;
                else
                    s.Command = command;
                    
                if (valuea != null)
                {
                    var val = double.Parse(valuea, UnitsConverter.DefaultNumberFormat);
                    s.obdValue = (p) => p.get(0) * val + offset;
                }
                if (valueb != null)
                {
                    var val = double.Parse(valueb, UnitsConverter.DefaultNumberFormat);
                    s.obdValue = (p) => p.get(1) * val + offset;
                }
                if (valuec != null)
                {
                    var val = double.Parse(valuec, UnitsConverter.DefaultNumberFormat);
                    s.obdValue = (p) => p.get(2) * val + offset;
                }
                if (valued != null)
                {
                    var val = double.Parse(valued, UnitsConverter.DefaultNumberFormat);
                    s.obdValue = (p) => p.get(3) * val + offset;
                }
                if (valueab != null)
                {
                    var val = double.Parse(valueab, UnitsConverter.DefaultNumberFormat);
                    s.obdValue = (p) => p.getab() * val + offset;
                }
                if (valuebc != null)
                {
                    var val = double.Parse(valuebc, UnitsConverter.DefaultNumberFormat);
                    s.obdValue = (p) => p.getbc() * val + offset;
                }
                if (valuecd != null)
                {
                    var val = double.Parse(valuecd, UnitsConverter.DefaultNumberFormat);
                    s.obdValue = (p) => p.getcd() * val + offset;
                }
                sensor = s;
            }else{
                // Custom derived sensor
                var s = new DerivedSensor("", basename, null);
                if (valuea != null)
                {
                    var val = double.Parse(valuea, UnitsConverter.DefaultNumberFormat);
                    s.DerivedValue = (a,b) => a.Value * val + offset;
                }
                sensor = s;                
            }
            sensor.ID = this.Namespace+"."+id;
            sensor.Name = id;
            //sensor.Display = display;
            if (units != null)
                sensor.Units = units;

            registry.Add(sensor);

            reader.ReadEndElement();
        }

        reader.ReadEndElement();
        reader.Close();
        reader = null;
    }
    
    public string GetName()
    {
        return "ECUXMLSensorProvider_" +Namespace;
    }

    public string GetDescription()
    {
        return "ECUXMLSensorProvider_" +Description;
    }
    
    public string GetDescription(string lang)
    {
        return GetDescription();
    }

    public void Activate(SensorRegistry registry)
    {
        init(registry);
    }

}

}

