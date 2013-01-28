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

            bool signed = false;
            double scale = 1;
            double offset = 0;
            int bit = -1;

            while(reader.NodeType == XmlNodeType.Element)
            {
                try{
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
                            scale = double.Parse(value, UnitsConverter.DefaultNumberFormat);
                            replyoffset = 0;
                            break;
                        case "valueb":
                            value = reader.ReadElementString();
                            scale = double.Parse(value, UnitsConverter.DefaultNumberFormat);
                            replyoffset = 1;
                            break;
                        case "valuec":
                            value = reader.ReadElementString();
                            scale = double.Parse(value, UnitsConverter.DefaultNumberFormat);
                            replyoffset = 2;
                            break;
                        case "valued":
                            value = reader.ReadElementString();
                            scale = double.Parse(value, UnitsConverter.DefaultNumberFormat);
                            replyoffset = 3;
                            break;
    
                        case "valueab":
                            word = reader.ReadElementString();
                            scale = double.Parse(word, UnitsConverter.DefaultNumberFormat);
                            replyoffset = 0;
                            break;
                        case "valuebc":
                            word = reader.ReadElementString();
                            scale = double.Parse(word, UnitsConverter.DefaultNumberFormat);
                            replyoffset = 1;
                            break;
                        case "valuecd":
                            word = reader.ReadElementString();
                            scale = double.Parse(word, UnitsConverter.DefaultNumberFormat);
                            replyoffset = 2;
                            break;
    
                        case "signed":
                            signed = true;
                            reader.ReadElementString();
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
                                scale = double.Parse(value, UnitsConverter.DefaultNumberFormat);
                            }else
                            if (reader.Name.StartsWith("word-"))
                            {
                                replyoffset = int.Parse(reader.Name.Replace("word-",""));
                                word = reader.ReadElementContentAsString();
                                scale = double.Parse(word, UnitsConverter.DefaultNumberFormat);
                            }else
                            if (reader.Name.StartsWith("wordle-"))
                            {
                                replyoffset = int.Parse(reader.Name.Replace("wordle-",""));
                                wordle = reader.ReadElementContentAsString();
                                scale = double.Parse(wordle, UnitsConverter.DefaultNumberFormat);
                            }else
                            if (reader.Name.StartsWith("dword-"))
                            {
                                replyoffset = int.Parse(reader.Name.Replace("dword-",""));
                                dword = reader.ReadElementContentAsString();
                                scale = double.Parse(dword, UnitsConverter.DefaultNumberFormat);
                            }else
                            if (reader.Name.StartsWith("dwordle-"))
                            {
                                replyoffset = int.Parse(reader.Name.Replace("dwordle-",""));
                                dwordle = reader.ReadElementContentAsString();
                                scale = double.Parse(dwordle, UnitsConverter.DefaultNumberFormat);
                            }else
                            {
                                throw new Exception("unknown tag `"+reader.Name+"` while creating PID "+id);
                            }
                            break;
                    }
                }catch(Exception e){
                    Logger.error("ECUXMLSensorProvider", "bad sensor param: "+id, e);
                }
            }

            Func<double, double> evaluator = (v) => {
                if (signed){
                    if (dword != null || dwordle != null)
                        v = (double)(int)((uint)v);
                    else if (word != null || wordle != null)
                        v = (double)(short)((ushort)v);
                    else
                        v = (double)(sbyte)((byte)v);
                }
                var res = v * scale + offset;
                if (bit != -1)
                    res = ((int)res >> bit)&1;
                return res;
            };

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
                    s.DerivedValue = (a,b) => evaluator(a.Value);
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
                    s.DerivedValue = (a,b) => evaluator((a as OBD2Sensor).getraw(replyoffset));
                }
                if (word != null)
                {
                    s.DerivedValue = (a,b) => evaluator((a as OBD2Sensor).getraw_word(replyoffset));
                }
                if (wordle != null)
                {
                    s.DerivedValue = (a,b) => evaluator((a as OBD2Sensor).getraw_wordle(replyoffset));
                }
                if (dword != null)
                {
                    s.DerivedValue = (a,b) => evaluator((a as OBD2Sensor).getraw_dword(replyoffset));
                }
                if (dwordle != null)
                {
                    s.DerivedValue = (a,b) => evaluator((a as OBD2Sensor).getraw_dwordle(replyoffset));
                }
                sensor = s;                
            }
            // command / raw command
            else if (basename == null)
            {
                var s = new OBD2Sensor();
                    
                if (value != null)
                {
                    s.obdValue = (p) => evaluator(p.get(replyoffset));
                }
                if (word != null)
                {
                    s.obdValue = (p) => evaluator(p.get_word(replyoffset));
                }
                sensor = s;
            }
            
            if (sensor != null && sensor is OBD2Sensor)
            {
                if (rawcommand != null)
                    (sensor as OBD2Sensor).RawCommand = rawcommand;
                else if (command != 0)
                    (sensor as OBD2Sensor).Command = command;
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

