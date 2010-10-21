using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

namespace hobd
{

public class ConfigVehicleData
{
    public string File{get; private set;}
    
    public string Name{get; private set;}
    public string Engine{get; private set;}
    
    List<string> sensors = new List<string>();
    public IEnumerable<string> Sensors{ get{ return sensors;}}
    
    public Dictionary<string, string> parameters = new Dictionary<string, string>();
    public IDictionary<string, string> Parameters{ get{ return parameters;}}
    
    public ConfigVehicleData(string file)
    {
        try{
            XmlReaderSettings xrs = new XmlReaderSettings();
            xrs.IgnoreWhitespace = true;
    
            this.File = file;
            XmlReader reader = XmlReader.Create(file, xrs);
            
            reader.ReadStartElement("vehicles");
    
            while( reader.IsStartElement("vehicle") ){
                reader.ReadStartElement();
                if ( reader.IsStartElement("obd") ){
                    reader.ReadStartElement();
                    this.Engine = reader.ReadElementString("engine");
                    this.sensors.Add(reader.ReadElementString("sensors"));
                }
                reader.ReadEndElement();
            }
            reader.ReadEndElement();
            reader.Close();
        }catch(Exception e){
            Logger.error(e);
        }
    }
    
    public override string ToString()   
    {
        return this.Name;
    }

}
    
public class ConfigData
{
    
    public string Port {get; set;}
    public string LogLevel  {get; set;}
    List<ConfigVehicleData> vehicles = new List<ConfigVehicleData>();
    public IEnumerable<ConfigVehicleData> Vehicles {get{ return vehicles;}}
    public string Vehicle {get; set;}
    public string Token {get; set;}
    public int DPI {get; private set;}
    public string Theme {get; private set;}
    public string Layout {get; private set;}
    
    string file;
    
    void init()
    {
        this.Port = "COM1";
        this.LogLevel = "ERROR";
        
        DPI = 0;
        Theme = "hobd.HOBDTheme";
        Layout = "default-landscape.layout";
    }    

    public ConfigData()
    {
        init();
    }
    
    public ConfigData(string file)
    {
        init();
        
        XmlReaderSettings xrs = new XmlReaderSettings();
        xrs.IgnoreWhitespace = true;

        this.file = file;
        XmlReader reader = XmlReader.Create(file, xrs);
        
        reader.ReadStartElement("hobd");

        while(true){
            if (reader.NodeType != XmlNodeType.Element){
                if (!reader.Read())
                    break;
                continue;
            }
            switch (reader.Name) {
                case "log-level":
                    this.LogLevel = reader.ReadElementContentAsString();    
                    break;
                case "port":
                    this.Port = reader.ReadElementContentAsString();
                    break;
                case "vehicles":
                    var v_file = reader.ReadElementContentAsString();
                    ConfigVehicleData v = new ConfigVehicleData(Path.Combine(Path.GetDirectoryName(file), v_file));
                    this.vehicles.Add(v);
                    break;
                case "vehicle":
                    this.Vehicle = reader.ReadElementContentAsString();
                    break;
                    
                case "drivehub-token":
                    this.Token = reader.ReadElementContentAsString();
                    break;                    
                case "dpi":
                    this.DPI = reader.ReadElementContentAsInt();
                    break;
                case "theme":
                    this.Theme = reader.ReadElementContentAsString();
                    break;
                case "layout":
                    this.Layout = reader.ReadElementContentAsString();
                    break;
                default:
                    reader.Read();
                    break;
            }
            
        }
        reader.Close();
        
    }
    
    public void Save()
    {
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        
        XmlWriter f = XmlWriter.Create(file, settings);
        
        f.WriteStartDocument();
        f.WriteStartElement("hobd");

            f.WriteElementString("log-level", this.LogLevel);

            f.WriteElementString("port", this.Port);

            vehicles.ForEach( (v) => f.WriteElementString("vehicles", v.File) );

            f.WriteElementString("vehicle", this.Vehicle);
            f.WriteElementString("drivehub-token", this.Token);

            if (this.DPI != 0)
                f.WriteElementString("dpi", this.DPI.ToString());
            f.WriteElementString("theme", this.Theme);
            f.WriteElementString("layout", this.Layout);

        f.WriteEndElement();
        f.WriteEndDocument();
        f.Close();

    }
    
    
}

}
