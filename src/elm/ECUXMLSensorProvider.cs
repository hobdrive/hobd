using System;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Reflection;

namespace hobd{

public class ECUXMLSensorProvider : SensorProvider
{
    string Path;

    public string Namespace{get; set;}
    public string Description{get; set;}

    XmlReader reader;

    public ECUXMLSensorProvider(string file)
    {
        this.Path = System.IO.Path.GetDirectoryName(file);
        XmlReaderSettings xrs = new XmlReaderSettings();
        xrs.IgnoreWhitespace = true;
        xrs.IgnoreComments = true;
        reader = XmlReader.Create(file, xrs);
    }
    
    void init(SensorRegistry registry)
    {
        if (reader == null)
            throw new Exception("Can't do double init");

        reader.ReadToFollowing("parameters");

        this.Namespace = reader.GetAttribute("namespace") ?? "Default";
        this.Description = reader.GetAttribute("description") ?? "";

        reader.ReadStartElement("parameters");

        while( reader.NodeType == XmlNodeType.Element ){

            if (reader.Name == "include")
            {
                var name = reader.ReadElementString().Trim();
                var ecuinc = new ECUXMLSensorProvider(System.IO.Path.Combine(Path, name));
                ecuinc.init(registry);
                continue;
            }

            if (reader.Name != "parameter")
            {
                reader.ReadElementString();
                continue;
            }
            
            var id = reader.GetAttribute("id");
            var display = reader.GetAttribute("display");

            reader.ReadStartElement();

            int command = 0;
            string clazz = null;
            string rawcommand = null;
            string basename = null;
            string basenameraw = null;
            int    replyoffset = 0;
            string units = null;

            string value = null;
            string word = null;
            string wordle = null;
            string dword = null;
            string dwordle = null;

            double offset = 0;
            int bit = -1;

            while(reader.NodeType == XmlNodeType.Element)
            {
                switch(reader.Name)
                {
                    case "class":
                        clazz = reader.ReadElementString().Trim();
                        break;
                    case "address":
                        reader.ReadStartElement();
                        var hexval = reader.ReadElementString("byte").Trim();
                        if (hexval.StartsWith("0x"))
                            hexval = hexval.Substring(2);
                        command = int.Parse(hexval, NumberStyles.HexNumber);
                        reader.ReadEndElement();
                        break;
                    case "raw":
                        rawcommand = reader.ReadElementString().Trim().Replace(";", "\r");
                        break;
                    case "base":
                        basename = reader.ReadElementString().Trim();
                        break;
                    case "base-raw":
                        basenameraw = reader.ReadElementString().Trim();
                        break;

                    case "value":
                    case "valuea":
                        value = reader.ReadElementString();
                        replyoffset = 0;
                        break;
                    case "valueb":
                        value = reader.ReadElementString();
                        replyoffset = 1;
                        break;
                    case "valuec":
                        value = reader.ReadElementString();
                        replyoffset = 2;
                        break;
                    case "valued":
                        value = reader.ReadElementString();
                        replyoffset = 3;
                        break;

                    case "valueab":
                        word = reader.ReadElementString();
                        replyoffset = 0;
                        break;
                    case "valuebc":
                        word = reader.ReadElementString();
                        replyoffset = 1;
                        break;
                    case "valuecd":
                        word = reader.ReadElementString();
                        replyoffset = 2;
                        break;

                    case "offset":
                        offset = double.Parse(reader.ReadElementString(), UnitsConverter.DefaultNumberFormat);
                        break;
                    case "bit":
                        bit = int.Parse(reader.ReadElementString());
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
                        if (reader.Name.StartsWith("value-"))
                        {
                            replyoffset = int.Parse(reader.Name.Replace("value-",""));
                            value = reader.ReadElementContentAsString();
                        }else
                        if (reader.Name.StartsWith("word-"))
                        {
                            replyoffset = int.Parse(reader.Name.Replace("word-",""));
                            word = reader.ReadElementContentAsString();
                        }else
                        if (reader.Name.StartsWith("wordle-"))
                        {
                            replyoffset = int.Parse(reader.Name.Replace("wordle-",""));
                            wordle = reader.ReadElementContentAsString();
                        }else
                        if (reader.Name.StartsWith("dword-"))
                        {
                            replyoffset = int.Parse(reader.Name.Replace("dword-",""));
                            dword = reader.ReadElementContentAsString();
                        }else
                        if (reader.Name.StartsWith("dwordle-"))
                        {
                            replyoffset = int.Parse(reader.Name.Replace("dwordle-",""));
                            dwordle = reader.ReadElementContentAsString();
                        }else
                        {
                            throw new Exception("unknown tag `"+reader.Name+"` while creating PID "+id);
                        }
                        break;
                }
            }

            CoreSensor sensor = null;
            if (clazz != null)
            {
                sensor = (CoreSensor)registry.CreateObject(clazz);
            }
            // OBD2 derived sensor
            else if (basename != null)
            {
                // Custom derived sensor
                var s = new DerivedSensor("", basename, null);
                if (value != null)
                {
                    var val = double.Parse(value, UnitsConverter.DefaultNumberFormat);
                    s.DerivedValue = (a,b) => {
                        var v = a.Value * val + offset;
                        if (bit != -1)
                            v = ((int)v >> bit)&1;
                        return v;
                    };
                }
                sensor = s;                
            }
            // RAW data from base sensor
            else if (basenameraw != null)
            {
                // Custom derived sensor
                var s = new DerivedSensor("", basenameraw, null);
                if (value != null)
                {
                    var val = double.Parse(value, UnitsConverter.DefaultNumberFormat);
                    s.DerivedValue = (a,b) => {
                        var v = (a as OBD2Sensor).getraw(replyoffset) * val + offset;
                        if (bit != -1)
                            v = ((int)v >> bit)&1;
                        return v;
                    };
                }
                if (word != null)
                {
                    var val = double.Parse(word, UnitsConverter.DefaultNumberFormat);
                    s.DerivedValue = (a,b) => {
                        var v = (a as OBD2Sensor).getraw_word(replyoffset) * val + offset;
                        if (bit != -1)
                            v = ((int)v >> bit)&1;
                        return v;
                    };
                }
                if (wordle != null)
                {
                    var val = double.Parse(wordle, UnitsConverter.DefaultNumberFormat);
                    s.DerivedValue = (a,b) => {
                        var v = (a as OBD2Sensor).getraw_wordle(replyoffset) * val + offset;
                        if (bit != -1)
                            v = ((int)v >> bit)&1;
                        return v;
                    };
                }
                if (dword != null)
                {
                    var val = double.Parse(dword, UnitsConverter.DefaultNumberFormat);
                    s.DerivedValue = (a,b) => {
                        var v = (a as OBD2Sensor).getraw_dword(replyoffset) * val + offset;
                        if (bit != -1)
                            v = ((int)v >> bit)&1;
                        return v;
                    };
                }
                if (dwordle != null)
                {
                    var val = double.Parse(dwordle, UnitsConverter.DefaultNumberFormat);
                    s.DerivedValue = (a,b) => {
                        var v = (a as OBD2Sensor).getraw_dwordle(replyoffset) * val + offset;
                        if (bit != -1)
                            v = ((int)v >> bit)&1;
                        return v;
                    };
                }
                sensor = s;                
            }
            // command / raw command
            else if (basename == null)
            {
                var s = new OBD2Sensor();
                if (rawcommand != null)
                    s.RawCommand = rawcommand;
                else
                    s.Command = command;
                    
                if (value != null)
                {
                    var val = double.Parse(value, UnitsConverter.DefaultNumberFormat);
                    s.obdValue = (p) => p.get(replyoffset) * val + offset;
                }
                if (word != null)
                {
                    var val = double.Parse(word, UnitsConverter.DefaultNumberFormat);
                    s.obdValue = (p) => p.get_word(replyoffset) * val + offset;
                }
                sensor = s;
            }
            if (sensor != null)
            {
                sensor.ID = this.Namespace+"."+id;
                sensor.Name = id;
                //sensor.Display = display;
                if (units != null)
                    sensor.Units = units;

                registry.Add(sensor);
            }
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

